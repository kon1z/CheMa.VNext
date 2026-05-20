namespace CheMa.VNext.ExternalServices;

/// <summary>
/// 外部服务 HTTP 成功响应中的业务错误检测器。
/// </summary>
public interface IExternalServiceBusinessErrorDetector
{
    /// <summary>
    /// 尝试从响应体中识别外部服务业务错误。
    /// </summary>
    bool TryDetect(string responseBody, out ExternalServiceBusinessError businessError);
}
