using System.Text.Json.Serialization;

namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿新增车辆请求。
/// </summary>
public class MaiHongVehicleCreateRequest
{
    [JsonPropertyName("plateNumber")]
    public string? PlateNumber { get; set; }

    [JsonPropertyName("vin")]
    public string? Vin { get; set; }

    [JsonPropertyName("brand_id")]
    public string? BrandId { get; set; }

    [JsonPropertyName("style_id")]
    public string? StyleId { get; set; }

    [JsonPropertyName("model_id")]
    public string? ModelId { get; set; }

    [JsonPropertyName("groupCode")]
    public string? GroupCode { get; set; }

    [JsonPropertyName("engineNumber")]
    public string? EngineNumber { get; set; }

    [JsonPropertyName("equipmentCode")]
    public string? EquipmentCode { get; set; }

    [JsonPropertyName("purchaseDate")]
    public string? PurchaseDate { get; set; }
}