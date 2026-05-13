using System.Text.Json.Serialization;

namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿通用数据响应。
/// </summary>
public class MaiHongResponse<T> : MaiHongResponse
{
    /// <summary>
    /// 响应数据。
    /// </summary>
    [JsonPropertyName("data")]
    public T? Data { get; set; }
}
