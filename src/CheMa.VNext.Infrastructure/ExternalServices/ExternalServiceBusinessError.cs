namespace CheMa.VNext.ExternalServices;

/// <summary>
/// 外部服务业务错误。
/// </summary>
public sealed class ExternalServiceBusinessError
{
    /// <summary>
    /// 外部服务返回的错误码。
    /// </summary>
    public required string ExternalErrorCode { get; init; }

    /// <summary>
    /// 外部服务返回的错误信息。
    /// </summary>
    public required string ExternalErrorMessage { get; init; }

    /// <summary>
    /// 映射后的系统业务异常码。
    /// </summary>
    public string? BusinessExceptionCode { get; init; }
}
