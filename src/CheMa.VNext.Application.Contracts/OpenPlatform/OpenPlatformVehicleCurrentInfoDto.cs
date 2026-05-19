using System;

namespace CheMa.VNext.OpenPlatform;

public class OpenPlatformVehicleCurrentInfoDto
{
    public string? Vin { get; set; }

    public decimal? Lat { get; set; }

    public decimal? Lon { get; set; }

    public decimal? Altitude { get; set; }

    public decimal? Speed { get; set; }

    public decimal? Direction { get; set; }

    public int? Door1Status { get; set; }

    public int? Door2Status { get; set; }

    public int? Door3Status { get; set; }

    public int? Door4Status { get; set; }

    public int? Door5Status { get; set; }

    public int? LockStatus { get; set; }

    public int? Engine { get; set; }

    public int? Bonnet { get; set; }

    public int? IgnitionStatus { get; set; }

    public decimal? OriginalMileage { get; set; }

    public decimal? FuelLevel { get; set; }

    public string? FuelLevelUnit { get; set; }

    public int? TearDownAlarm { get; set; }

    public int? AccStatus { get; set; }

    public int? LocationStatus { get; set; }

    public DateTime? ReportTime { get; set; }

    public DateTime? SendTime { get; set; }

    public DateTime? ReceiveTime { get; set; }

    public decimal? Soc { get; set; }

    public string? CarType { get; set; }

    public decimal? ContinueVoyage { get; set; }

    public int? ChargeStatus { get; set; }

    public string? ChargeType { get; set; }

    public decimal? CarVoltage { get; set; }

    public DateTime? LowTireTime { get; set; }

    public int? HighTemp { get; set; }

    public decimal? TireTempFl { get; set; }

    public decimal? TireTempFr { get; set; }

    public decimal? TireTempRl { get; set; }

    public decimal? TireTempRr { get; set; }

    public int? TireTempStatusFl { get; set; }

    public int? TireTempStatusFr { get; set; }

    public int? TireTempStatusRl { get; set; }

    public int? TireTempStatusRr { get; set; }

    public string? GearStatus { get; set; }

    public int? CarFortificationStatus { get; set; }

    public int? BrakeStatus { get; set; }

    public int? AirConditionerStatus { get; set; }

    public int? FootBrakeStatus { get; set; }

    public int? Light1Status { get; set; }

    public int? Light2Status { get; set; }

    public int? Window1status { get; set; }

    public int? Window2status { get; set; }

    public int? Window3status { get; set; }

    public int? Window4status { get; set; }

    public int? Window5status { get; set; }

    public int? SeatVentilationStatus { get; set; }

    public int? SeatHeating { get; set; }

    public decimal? TotalAverageFuel { get; set; }
}
