using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿车系。
/// </summary>
public class MaiHongStyleDto
{
    [JsonPropertyName("styleId")]
    public int? StyleId { get; set; }

    [JsonPropertyName("styleName")]
    public string? StyleName { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}