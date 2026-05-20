using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CheMa.VNext.ExternalServices;

/// <summary>
/// 将外部服务 HTTP 2xx 响应中的业务错误转换为系统业务异常。
/// </summary>
public sealed class ExternalServiceBusinessExceptionHandler : DelegatingHandler
{
    private const int MaxBodyPreviewLength = 2048;

    private readonly string _serviceName;
    private readonly IExternalServiceBusinessErrorDetector _detector;
    private readonly ILogger<ExternalServiceBusinessExceptionHandler> _logger;

    /// <summary>
    /// 初始化外部服务业务异常处理器。
    /// </summary>
    public ExternalServiceBusinessExceptionHandler(
        string serviceName,
        IExternalServiceBusinessErrorDetector detector,
        ILogger<ExternalServiceBusinessExceptionHandler> logger)
    {
        _serviceName = serviceName;
        _detector = detector;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode || response.Content is null)
        {
            return response;
        }

        var mediaType = response.Content.Headers.ContentType?.MediaType;
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        ResetResponseContent(response, responseBody, mediaType);

        if (!_detector.TryDetect(responseBody, out var businessError))
        {
            return response;
        }

        var operation = request.RequestUri?.AbsolutePath;
        var bodyPreview = CreateBodyPreview(responseBody);

        _logger.LogWarning(
            "External service {ExternalService} returned business failure. Operation: {ExternalOperation}, HttpMethod: {HttpMethod}, HttpStatusCode: {HttpStatusCode}, ExternalErrorCode: {ExternalErrorCode}, ExternalErrorMessage: {ExternalErrorMessage}",
            _serviceName,
            operation,
            request.Method.Method,
            (int)response.StatusCode,
            businessError.ExternalErrorCode,
            businessError.ExternalErrorMessage);

        Activity.Current?.SetTag("external.system", _serviceName);
        Activity.Current?.SetTag("external.operation", operation);
        Activity.Current?.SetTag("external.business.success", false);
        Activity.Current?.SetTag("external.business.code", businessError.ExternalErrorCode);
        Activity.Current?.SetTag("external.business.message", businessError.ExternalErrorMessage);

        throw new ExternalServiceBusinessException(
            businessError.BusinessExceptionCode ?? VNextDomainErrorCodes.ExternalServiceBusinessError,
            _serviceName,
            businessError.ExternalErrorCode,
            businessError.ExternalErrorMessage,
            (int)response.StatusCode,
            operation,
            bodyPreview);
    }

    private static void ResetResponseContent(HttpResponseMessage response, string responseBody, string? mediaType)
    {
        response.Content.Dispose();
        response.Content = string.IsNullOrWhiteSpace(mediaType)
            ? new StringContent(responseBody, Encoding.UTF8)
            : new StringContent(responseBody, Encoding.UTF8, mediaType);
    }

    private static string CreateBodyPreview(string body)
    {
        return body.Length <= MaxBodyPreviewLength
            ? body
            : body[..MaxBodyPreviewLength];
    }
}
