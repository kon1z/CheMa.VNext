using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿车辆定位。
/// </summary>
public class MaiHongPositionDto
{
    [JsonPropertyName("vehicleId")]
    public JsonElement? VehicleId { get; set; }

    [JsonPropertyName("reportTime")]
    public string? ReportTime { get; set; }

    [JsonPropertyName("onlineState")]
    public string? OnlineState { get; set; }

    [JsonPropertyName("gpsState")]
    public string? GpsState { get; set; }

    [JsonPropertyName("lon")]
    public string? Lon { get; set; }

    [JsonPropertyName("lat")]
    public string? Lat { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}