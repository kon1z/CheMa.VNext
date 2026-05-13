using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿车辆控制请求。
/// </summary>
public class MaiHongVehicleControlRequest
{
    [JsonPropertyName("vehicleHwid")]
    public string? VehicleHwid { get; set; }

    [JsonPropertyName("order")]
    public Dictionary<string, int> Order { get; set; } = new();
}