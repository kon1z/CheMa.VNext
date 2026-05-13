using System.Text.Json.Serialization;

namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿车辆定位数据。
/// </summary>
public class MaiHongPositionsDataDto
{
    [JsonPropertyName("positions")]
    public MaiHongPositionDto[]? Positions { get; set; }
}