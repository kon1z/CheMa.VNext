using System.Text.Json.Serialization;

namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿车辆有效结束时间响应。
/// </summary>
public class MaiHongEquipEndTimeResponse : MaiHongResponse
{
    [JsonPropertyName("equipEndTime")]
    public long? EquipEndTime { get; set; }
}