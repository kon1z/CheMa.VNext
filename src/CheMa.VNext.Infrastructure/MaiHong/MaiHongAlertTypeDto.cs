using System.Text.Json.Serialization;

namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿告警类型。
/// </summary>
public class MaiHongAlertTypeDto
{
    [JsonPropertyName("alertTypeId")]
    public string? AlertTypeId { get; set; }

    [JsonPropertyName("alertTypeName")]
    public string? AlertTypeName { get; set; }
}