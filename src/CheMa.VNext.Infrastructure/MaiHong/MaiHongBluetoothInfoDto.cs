using System.Text.Json.Serialization;

namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿蓝牙信息。
/// </summary>
public class MaiHongBluetoothInfoDto : MaiHongResponse
{
    [JsonPropertyName("bluetoothName")]
    public string? BluetoothName { get; set; }

    [JsonPropertyName("bluetoothPwd")]
    public string? BluetoothPwd { get; set; }
}