using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿行程。
/// </summary>
public class MaiHongTripDto
{
    [JsonPropertyName("tripId")]
    public JsonElement? TripId { get; set; }

    [JsonPropertyName("startTime")]
    public string? StartTime { get; set; }

    [JsonPropertyName("endTime")]
    public string? EndTime { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}