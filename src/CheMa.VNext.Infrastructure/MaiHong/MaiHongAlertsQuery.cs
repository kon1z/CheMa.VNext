namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿告警历史查询参数。
/// </summary>
public class MaiHongAlertsQuery : MaiHongTripsQuery
{
    public int Page { get; set; } = 1;

    public int? PageSize { get; set; }

    public int? Order { get; set; }

    public string? AlertType { get; set; }
}