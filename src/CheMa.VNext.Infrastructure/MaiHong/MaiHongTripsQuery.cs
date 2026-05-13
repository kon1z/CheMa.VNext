namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿行程查询参数。
/// </summary>
public class MaiHongTripsQuery
{
    public string VehicleHwid { get; set; } = default!;

    public string DateFrom { get; set; } = default!;

    public string DateTo { get; set; } = default!;
}