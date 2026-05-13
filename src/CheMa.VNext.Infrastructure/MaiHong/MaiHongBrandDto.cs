using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿品牌。
/// </summary>
public class MaiHongBrandDto
{
    [JsonPropertyName("brandId")]
    public int? BrandId { get; set; }

    [JsonPropertyName("brandName")]
    public string? BrandName { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}