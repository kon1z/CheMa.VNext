using System.Text.Json.Serialization;

namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿告警类型数据。
/// </summary>
public class MaiHongAlertTypesDataDto
{
    [JsonPropertyName("alertTypes")]
    public MaiHongAlertTypeDto[]? AlertTypes { get; set; }
}