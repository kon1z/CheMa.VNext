using System.Text.Json;
using System.Text.Json.Serialization;

namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿通用响应。
/// </summary>
public class MaiHongResponse
{
    /// <summary>
    /// 错误码。
    /// </summary>
    [JsonPropertyName("errno")]
    public JsonElement? Errno { get; set; }

    /// <summary>
    /// 错误信息。
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }
}