using System.Text.Json;
using System.Text.Json.Serialization;

namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿车辆参数响应。
/// </summary>
public class MaiHongSettingResponse
{
    [JsonPropertyName("resultCode")]
    public JsonElement? ResultCode { get; set; }

    [JsonPropertyName("resultDetail")]
    public string? ResultDetail { get; set; }

    [JsonPropertyName("data")]
    public MaiHongVehicleSettingDto? Data { get; set; }
}