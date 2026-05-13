using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿车型。
/// </summary>
public class MaiHongModelDto
{
    [JsonPropertyName("modelId")]
    public int? ModelId { get; set; }

    [JsonPropertyName("modelName")]
    public string? ModelName { get; set; }

    [JsonPropertyName("year")]
    public string? Year { get; set; }

    [JsonPropertyName("displacement")]
    public string? Displacement { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}