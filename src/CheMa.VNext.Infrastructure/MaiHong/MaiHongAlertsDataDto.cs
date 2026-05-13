using System.Text.Json;
using System.Text.Json.Serialization;

namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿告警历史数据。
/// </summary>
public class MaiHongAlertsDataDto
{
    [JsonPropertyName("totalRecords")]
    public JsonElement? TotalRecords { get; set; }

    [JsonPropertyName("pageSize")]
    public JsonElement? PageSize { get; set; }

    [JsonPropertyName("pageNum")]
    public JsonElement? PageNum { get; set; }

    [JsonPropertyName("alerts")]
    public MaiHongAlertDto[]? Alerts { get; set; }
}