using System.Text.Json;
using System.Text.Json.Serialization;

namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿轨迹数据。
/// </summary>
public class MaiHongTracesDataDto
{
    [JsonPropertyName("totalMileage")]
    public JsonElement? TotalMileage { get; set; }

    [JsonPropertyName("avgSpeed")]
    public JsonElement? AvgSpeed { get; set; }

    [JsonPropertyName("FCPH")]
    public JsonElement? Fcph { get; set; }

    [JsonPropertyName("trace")]
    public MaiHongTracePointDto[]? Trace { get; set; }
}