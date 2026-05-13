using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿车身状态。
/// </summary>
public class MaiHongVehicleStatusDto
{
    [JsonPropertyName("vehicleId")]
    public JsonElement? VehicleId { get; set; }

    [JsonPropertyName("reportTime")]
    public string? ReportTime { get; set; }

    [JsonPropertyName("engine")]
    public string? Engine { get; set; }

    [JsonPropertyName("lock")]
    public string? Lock { get; set; }

    [JsonPropertyName("isInDefend")]
    public string? IsInDefend { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}