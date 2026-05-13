using System.Text.Json;
using System.Text.Json.Serialization;

namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿命令结果响应。
/// </summary>
public class MaiHongCommandResultResponse
{
    [JsonPropertyName("result")]
    public JsonElement? Result { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("licensePlate")]
    public string? LicensePlate { get; set; }

    [JsonPropertyName("postTime")]
    public string? PostTime { get; set; }
}