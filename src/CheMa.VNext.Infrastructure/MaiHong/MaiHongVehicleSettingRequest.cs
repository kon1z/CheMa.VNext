using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿车辆参数设置请求。
/// </summary>
public class MaiHongVehicleSettingRequest
{
    [JsonExtensionData]
    public Dictionary<string, object?> Values { get; set; } = new();
}