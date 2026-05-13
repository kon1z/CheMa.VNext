namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿车联网开放接口配置。
/// </summary>
public class MaiHongGatewayOptions
{
    /// <summary>
    /// 配置节名称。
    /// </summary>
    public const string SectionName = "MaiHong";

    /// <summary>
    /// 迈鸿服务基础地址，例如 http://117.78.36.98。
    /// </summary>
    public string BaseUrl { get; set; } = "http://117.78.36.98";

    /// <summary>
    /// WSSE 用户名。
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 迈鸿分配的 API Key。生产环境应通过环境变量、AgileConfig 或部署系统注入。
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// WSSE 摘要参与签名的 URI。
    /// </summary>
    public string DigestUri { get; set; } = "api/hwCallBack";

    /// <summary>
    /// HTTP 请求超时时间，单位秒。
    /// </summary>
    public int TimeoutSeconds { get; set; } = 10;
}
