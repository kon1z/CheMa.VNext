using System.Text.Json.Serialization;

namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿设置蓝牙名称请求。
/// </summary>
public class MaiHongSetBluetoothNameRequest
{
    [JsonPropertyName("vehicleId")]
    public string? VehicleId { get; set; }

    [JsonPropertyName("bluetoothName")]
    public string? BluetoothName { get; set; }

    [JsonPropertyName("key")]
    public string? Key { get; set; }
}