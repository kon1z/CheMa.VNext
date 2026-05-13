using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿车辆参数设置响应。
/// </summary>
public class MaiHongSettingResultResponse
{
    [JsonPropertyName("resultCode")]
    public JsonElement? ResultCode { get; set; }

    [JsonPropertyName("resultDetail")]
    public string? ResultDetail { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}