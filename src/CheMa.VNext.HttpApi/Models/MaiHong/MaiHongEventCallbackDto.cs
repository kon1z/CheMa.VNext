using System.Text.Json.Serialization;

namespace CheMa.VNext.Models.MaiHong;

/// <summary>
/// 迈鸿事件回调内容。
/// </summary>
public class MaiHongEventCallbackDto
{
    /// <summary>
    /// 事件类型，1 为告警，2 为通知。
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// 事件子类型。
    /// </summary>
    [JsonPropertyName("subType")]
    public string? SubType { get; set; }

    /// <summary>
    /// 车辆 hwid。
    /// </summary>
    [JsonPropertyName("object")]
    public string? Object { get; set; }

    /// <summary>
    /// 事件内容。
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    /// <summary>
    /// 事件时间。
    /// </summary>
    [JsonPropertyName("time")]
    public string? Time { get; set; }

    /// <summary>
    /// 经度。
    /// </summary>
    [JsonPropertyName("lon")]
    public string? Lon { get; set; }

    /// <summary>
    /// 纬度。
    /// </summary>
    [JsonPropertyName("lat")]
    public string? Lat { get; set; }
}
