namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿轨迹查询参数。
/// </summary>
public class MaiHongTracesQuery : MaiHongTripsQuery
{
    public int Type { get; set; }

    public string TripId { get; set; } = default!;
}