using Volo.Abp;

namespace CheMa.VNext.ExternalServices;

/// <summary>
/// 外部服务返回业务失败时抛出的系统业务异常。
/// </summary>
public class ExternalServiceBusinessException : BusinessException
{
    /// <summary>
    /// 外部服务名称。
    /// </summary>
    public string ServiceName { get; }

    /// <summary>
    /// 外部服务操作。
    /// </summary>
    public string? Operation { get; }

    /// <summary>
    /// 外部服务返回的错误码。
    /// </summary>
    public string ExternalErrorCode { get; }

    /// <summary>
    /// 外部服务返回的错误信息。
    /// </summary>
    public string ExternalErrorMessage { get; }

    /// <summary>
    /// HTTP 状态码。
    /// </summary>
    public int? HttpStatusCode { get; }

    /// <summary>
    /// 响应体预览。
    /// </summary>
    public string? ResponseBodyPreview { get; }

    /// <summary>
    /// 初始化外部服务业务异常。
    /// </summary>
    public ExternalServiceBusinessException(
        string code,
        string serviceName,
        string externalErrorCode,
        string externalErrorMessage,
        int? httpStatusCode = null,
        string? operation = null,
        string? responseBodyPreview = null)
        : base(code, externalErrorMessage)
    {
        ServiceName = serviceName;
        Operation = operation;
        ExternalErrorCode = externalErrorCode;
        ExternalErrorMessage = externalErrorMessage;
        HttpStatusCode = httpStatusCode;
        ResponseBodyPreview = responseBodyPreview;

        WithData(nameof(ServiceName), serviceName);
        WithData(nameof(ExternalErrorCode), externalErrorCode);
        WithData(nameof(ExternalErrorMessage), externalErrorMessage);

        if (operation is not null)
        {
            WithData(nameof(Operation), operation);
        }

        if (httpStatusCode.HasValue)
        {
            WithData(nameof(HttpStatusCode), httpStatusCode.Value);
        }
    }
}
