using System;
using System.Text.Json.Serialization;

namespace CheMa.VNext.OpenPlatform.Dtos;

public class OpenPlatformVehicleInfoDto
{
    public string Vin { get; set; } = default!;

    public string? PlateNo { get; set; }

    public int? VehicleStatus { get; set; }

    public int? LockStatus { get; set; }

    public decimal? Mileage { get; set; }

    public decimal? FuelPercent { get; set; }

    public decimal? SocPercent { get; set; }

    public decimal? Longitude { get; set; }

    public decimal? Latitude { get; set; }

    public DateTime? GpsTime { get; set; }

    public DateTime? UpdateTime { get; set; }

    public string? Lat { get; set; }

    public string? Lon { get; set; }

    public string? Altitude { get; set; }

    public string? Speed { get; set; }

    public string? Direction { get; set; }

    public int? Door1Status { get; set; }

    public int? Door2Status { get; set; }

    public int? Door3Status { get; set; }

    public int? Door4Status { get; set; }

    public int? Door5Status { get; set; }

    public int? Engine { get; set; }

    public int? Bonnet { get; set; }

    public int? IgnitionStatus { get; set; }

    public string? OriginalMileage { get; set; }

    public string? FuelLevel { get; set; }

    public string? LocationStatus { get; set; }

    public DateTime? ReportTime { get; set; }

    public DateTime? SendTime { get; set; }

    public DateTime? ReceiveTime { get; set; }

    public string? ContinueVoyage { get; set; }

    public string? CarVoltage { get; set; }

    public int? CarFortificationStatus { get; set; }

    public int? BrakeStatus { get; set; }

    public int? FootBrakeStatus { get; set; }

    public int? Light1Status { get; set; }

    public int? Light2Status { get; set; }

    [JsonPropertyName("window1status")]
    public int? Window1Status { get; set; }

    [JsonPropertyName("window2status")]
    public int? Window2Status { get; set; }

    [JsonPropertyName("window3status")]
    public int? Window3Status { get; set; }

    [JsonPropertyName("window4status")]
    public int? Window4Status { get; set; }

    public string? TotalAverageFuel { get; set; }
}
