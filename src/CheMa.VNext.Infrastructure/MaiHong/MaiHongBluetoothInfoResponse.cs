using System.Text.Json.Serialization;

namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿蓝牙信息响应。
/// </summary>
public class MaiHongBluetoothInfoResponse
{
    [JsonPropertyName("list")]
    public MaiHongBluetoothInfoDto[]? List { get; set; }
}