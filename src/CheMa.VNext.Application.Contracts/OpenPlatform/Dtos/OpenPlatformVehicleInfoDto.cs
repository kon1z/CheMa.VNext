using System;

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
}
