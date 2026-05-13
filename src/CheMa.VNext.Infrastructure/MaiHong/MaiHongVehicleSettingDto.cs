using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿车辆参数。
/// </summary>
public class MaiHongVehicleSettingDto
{
    [JsonPropertyName("temperature")]
    public string? Temperature { get; set; }

    [JsonPropertyName("PM25")]
    public string? Pm25 { get; set; }

    [JsonPropertyName("sleepStatus")]
    public int? SleepStatus { get; set; }

    [JsonPropertyName("gpsFlag")]
    public int? GpsFlag { get; set; }

    [JsonPropertyName("autoLock")]
    public int? AutoLock { get; set; }

    [JsonPropertyName("sensitivity")]
    public int? Sensitivity { get; set; }

    [JsonPropertyName("startTime")]
    public int? StartTime { get; set; }

    [JsonPropertyName("dialSwitch")]
    public JsonElement? DialSwitch { get; set; }

    [JsonPropertyName("keyboardPassword")]
    public string? KeyboardPassword { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}