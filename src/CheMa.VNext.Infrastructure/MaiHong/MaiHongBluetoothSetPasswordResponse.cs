using System.Text.Json.Serialization;

namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿蓝牙密码设置响应。
/// </summary>
public class MaiHongBluetoothSetPasswordResponse
{
    [JsonPropertyName("list")]
    public MaiHongCommandResultResponse[]? List { get; set; }
}