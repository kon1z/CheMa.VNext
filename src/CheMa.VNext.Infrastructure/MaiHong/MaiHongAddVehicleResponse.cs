using System.Text.Json.Serialization;

namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿新增车辆响应。
/// </summary>
public class MaiHongAddVehicleResponse : MaiHongResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("hwId")]
    public string? HwId { get; set; }
}