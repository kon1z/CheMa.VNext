using System.Text.Json.Serialization;

namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿车辆信息。
/// </summary>
public class MaiHongVehicleDto : MaiHongVehicleCreateRequest
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("hwId")]
    public string? HwId { get; set; }
}