using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace CheMa.VNext.Logging;

public class HttpExchangeLoggingMiddleware
{
    private static readonly string[] SkippedPathPrefixes =
    [
        "/health",
        "/alive",
        "/swagger"
    ];

    private static readonly string[] StaticFileExtensions =
    [
        ".css",
        ".js",
        ".map",
        ".png",
        ".jpg",
        ".jpeg",
        ".gif",
        ".svg",
        ".ico",
        ".woff",
        ".woff2",
        ".ttf",
        ".eot"
    ];

    private readonly RequestDelegate _next;
    private readonly ILogger<HttpExchangeLoggingMiddleware> _logger;

    public HttpExchangeLoggingMiddleware(RequestDelegate next, ILogger<HttpExchangeLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var request = context.Request;
        var captureDecision = GetCaptureDecision(request.Path);
        var originalResponseBody = context.Response.Body;
        string? requestBody = null;
        string? responseBody = null;
        Exception? exception = null;

        try
        {
            if (captureDecision.CaptureBody)
            {
                requestBody = await ReadRequestBodyAsync(request);
                context.Response.Body = new MemoryStream();
            }

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                if (captureDecision.CaptureBody && context.Response.Body is MemoryStream responseStream)
                {
                    responseBody = await ReadResponseBodyAsync(responseStream);
                    responseStream.Position = 0;
                    await responseStream.CopyToAsync(originalResponseBody);
                    context.Response.Body = originalResponseBody;
                }
            }
        }
        finally
        {
            stopwatch.Stop();
            context.Response.Body = originalResponseBody;

            try
            {
                LogHttpExchange(context, captureDecision, requestBody, responseBody, stopwatch.Elapsed.TotalMilliseconds, exception);
            }
            catch (Exception logException)
            {
                _logger.LogError(logException,
                    "{event_type} {success} {exception_type} {exception_message}",
                    "log.pipeline.failure",
                    false,
                    logException.GetType().FullName,
                    logException.Message);
            }
        }
    }

    private static async Task<string?> ReadRequestBodyAsync(HttpRequest request)
    {
        if (request.ContentLength is null or 0 || !request.Body.CanRead)
        {
            return null;
        }

        request.EnableBuffering();
        request.Body.Position = 0;

        using var reader = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        return body;
    }

    private static async Task<string?> ReadResponseBodyAsync(MemoryStream responseStream)
    {
        if (responseStream.Length == 0)
        {
            return null;
        }

        responseStream.Position = 0;
        using var reader = new StreamReader(responseStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }

    private void LogHttpExchange(
        HttpContext context,
        CaptureDecision captureDecision,
        string? requestBody,
        string? responseBody,
        double durationMs,
        Exception? exception)
    {
        var activity = Activity.Current;
        var request = context.Request;
        var response = context.Response;
        var user = context.User;
        var routeEndpoint = context.GetEndpoint() as RouteEndpoint;
        var actionDescriptor = routeEndpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();
        var statusCode = response.StatusCode;
        var responseLength = response.ContentLength;
        if (responseLength == null && response.Body.CanSeek)
        {
            responseLength = response.Body.Length;
        }
        var success = exception == null && statusCode < StatusCodes.Status500InternalServerError;

        var fields = new Dictionary<string, object?>
        {
            ["event_type"] = "http.exchange",
            ["trace_id"] = activity?.TraceId.ToString(),
            ["span_id"] = activity?.SpanId.ToString(),
            ["parent_span_id"] = activity?.ParentSpanId.ToString(),
            ["correlation_id"] = GetCorrelationId(context),
            ["tenant_id"] = FindClaimValue(user, "tenant_id", "tenantid", "abp.tenantid"),
            ["tenant_name"] = FindClaimValue(user, "tenant_name", "tenantname", "abp.tenantname"),
            ["user_id"] = FindClaimValue(user, ClaimTypes.NameIdentifier, "sub"),
            ["user_name"] = FindClaimValue(user, ClaimTypes.Name, "name", "preferred_username"),
            ["client_id"] = FindClaimValue(user, "client_id", "azp"),
            ["level"] = exception == null ? "Information" : "Error",
            ["success"] = success,
            ["duration_ms"] = Math.Round(durationMs, 2),
            ["exception.type"] = exception?.GetType().FullName,
            ["exception.message"] = exception?.Message,
            ["exception.stacktrace"] = exception?.ToString(),
            ["http.request.method"] = request.Method,
            ["http.request.scheme"] = request.Scheme,
            ["http.request.host"] = request.Host.Value,
            ["http.request.path"] = request.Path.Value,
            ["http.request.query_string"] = request.QueryString.Value,
            ["http.route"] = routeEndpoint?.RoutePattern.RawText,
            ["http.status_code"] = statusCode,
            ["http.request.content_type"] = request.ContentType,
            ["http.response.content_type"] = response.ContentType,
            ["http.request.content_length"] = request.ContentLength,
            ["http.response.content_length"] = responseLength,
            ["http.request.headers"] = SerializeHeaders(request.Headers),
            ["http.response.headers"] = SerializeHeaders(response.Headers),
            ["http.request.header.user_agent"] = request.Headers.UserAgent.ToString(),
            ["http.request.header.authorization"] = request.Headers.Authorization.ToString(),
            ["http.request.header.cookie"] = request.Headers.Cookie.ToString(),
            ["http.request.header.content_type"] = request.Headers.ContentType.ToString(),
            ["http.request.header.x_forwarded_for"] = request.Headers["X-Forwarded-For"].ToString(),
            ["http.request.remote_ip"] = context.Connection.RemoteIpAddress?.ToString(),
            ["http.request.body"] = requestBody,
            ["http.response.body"] = responseBody,
            ["http.body.capture_mode"] = captureDecision.CaptureMode,
            ["http.body.capture_reason"] = captureDecision.Reason,
            ["abp.action.name"] = actionDescriptor?.ActionName ?? routeEndpoint?.DisplayName,
            ["abp.application.service"] = actionDescriptor?.ControllerTypeInfo.FullName
        };

        using (_logger.BeginScope(fields))
        {
            _logger.Log(success ? LogLevel.Information : LogLevel.Error,
                exception,
                "{event_type} {http_request_method} {http_request_path} {http_status_code} {duration_ms}ms",
                "http.exchange",
                request.Method,
                request.Path.Value,
                statusCode,
                Math.Round(durationMs, 2));
        }
    }

    private static CaptureDecision GetCaptureDecision(PathString path)
    {
        var value = path.Value ?? string.Empty;

        if (SkippedPathPrefixes.Any(prefix => value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            return new CaptureDecision(false, "skipped", "excluded_path");
        }

        if (StaticFileExtensions.Any(extension => value.EndsWith(extension, StringComparison.OrdinalIgnoreCase)))
        {
            return new CaptureDecision(false, "skipped", "static_resource");
        }

        if (value.StartsWith("/api/", StringComparison.OrdinalIgnoreCase) || value.Equals("/connect/token", StringComparison.OrdinalIgnoreCase))
        {
            return new CaptureDecision(true, "full", "included_path");
        }

        return new CaptureDecision(false, "summary", "not_in_full_body_whitelist");
    }

    private static string? GetCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-Correlation-Id", out var correlationId) && !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId.ToString();
        }

        if (context.Request.Headers.TryGetValue("Correlation-Id", out correlationId) && !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId.ToString();
        }

        return context.TraceIdentifier;
    }

    private static string? FindClaimValue(ClaimsPrincipal principal, params string[] claimTypes)
    {
        foreach (var claimType in claimTypes)
        {
            var value = principal.FindFirst(claimType)?.Value;
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    private static string SerializeHeaders(IHeaderDictionary headers)
    {
        var values = headers.ToDictionary(header => header.Key, header => header.Value.ToArray(), StringComparer.OrdinalIgnoreCase);
        return JsonSerializer.Serialize(values);
    }

    private sealed record CaptureDecision(bool CaptureBody, string CaptureMode, string Reason);
}

