using System.Text.Json.Serialization;

namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿设置蓝牙密码请求。
/// </summary>
public class MaiHongSetBluetoothPasswordRequest
{
    [JsonPropertyName("vehicleId")]
    public string? VehicleId { get; set; }

    [JsonPropertyName("bluetoothPwd")]
    public string? BluetoothPwd { get; set; }

    [JsonPropertyName("key")]
    public string? Key { get; set; }
}