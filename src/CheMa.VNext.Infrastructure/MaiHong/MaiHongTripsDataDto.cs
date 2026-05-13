using System.Text.Json;
using System.Text.Json.Serialization;

namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿行程数据。
/// </summary>
public class MaiHongTripsDataDto
{
    [JsonPropertyName("vehicleId")]
    public JsonElement? VehicleId { get; set; }

    [JsonPropertyName("trips")]
    public MaiHongTripDto[]? Trips { get; set; }
}