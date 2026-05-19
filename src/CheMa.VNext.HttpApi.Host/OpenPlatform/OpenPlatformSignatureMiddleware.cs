using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using CheMa.VNext.OpenPlatform.AppServices;
using CheMa.VNext.OpenPlatform.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace CheMa.VNext.OpenPlatform;

public class OpenPlatformSignatureMiddleware : IMiddleware
{
    private readonly IOpenPlatformSignatureService _openPlatformSignatureService;
    private readonly IOpenPlatformRequestContextAccessor _requestContextAccessor;
    private readonly OpenPlatformAccessLogWriter _accessLogWriter;
    private readonly IOptions<OpenPlatformOptions> _options;

    public OpenPlatformSignatureMiddleware(
        IOpenPlatformSignatureService openPlatformSignatureService,
        IOpenPlatformRequestContextAccessor requestContextAccessor,
        OpenPlatformAccessLogWriter accessLogWriter,
        IOptions<OpenPlatformOptions> options)
    {
        _openPlatformSignatureService = openPlatformSignatureService;
        _requestContextAccessor = requestContextAccessor;
        _accessLogWriter = accessLogWriter;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!_options.Value.Enabled || !context.Request.Path.StartsWithSegments(_options.Value.OpenApiPrefix, StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        var startedAt = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();
        Guid? openAppId = null;
        string? clientId = null;
        string? failureCode = null;
        string? failureMessage = null;
        var succeeded = false;

        try
        {
            var result = await _openPlatformSignatureService.ValidateAsync(context);
            openAppId = result.OpenApp.Id;
            clientId = result.ClientId;
            _requestContextAccessor.Current = new OpenPlatformRequestContext
            {
                OpenAppId = result.OpenApp.Id,
                ClientId = result.ClientId,
                AppName = result.OpenApp.Name,
                RequestTimestamp = result.Timestamp,
                Nonce = result.Nonce
            };

            await next(context);
            succeeded = context.Response.StatusCode < 400;
        }
        catch (OpenPlatformException ex)
        {
            clientId ??= context.Request.Headers[OpenPlatformRequestHeaders.ClientId];
            failureCode = ex.ErrorCode;
            failureMessage = ex.Message;
            context.Response.StatusCode = ex.StatusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                error = new
                {
                    code = ex.ErrorCode,
                    message = ex.Message
                }
            }));
        }
        finally
        {
            stopwatch.Stop();
            await _accessLogWriter.WriteAsync(new OpenPlatformAccessLogInfo
            {
                OpenAppId = openAppId,
                ClientId = clientId,
                RequestPath = context.Request.Path.Value ?? string.Empty,
                HttpMethod = context.Request.Method,
                QueryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : null,
                TraceId = context.TraceIdentifier,
                RemoteIpAddress = context.Connection.RemoteIpAddress?.ToString(),
                UserAgent = context.Request.Headers.UserAgent.ToString(),
                Timestamp = startedAt,
                ElapsedMs = stopwatch.ElapsedMilliseconds,
                Succeeded = succeeded,
                FailureCode = failureCode,
                FailureMessage = failureMessage,
                ResponseStatusCode = context.Response.StatusCode
            });
        }
    }
}
