using System;
using CheMa.VNext.Vehicles.Enums;

namespace CheMa.VNext.VehicleDevices.Dtos;

public class VehicleDeviceLocationDto
{
    public Guid VehicleId { get; set; }

    public decimal Longitude { get; set; }

    public decimal Latitude { get; set; }

    public string CoordinateSystem { get; set; } = VehicleDeviceConsts.CoordinateSystemBd09;

    public decimal? Speed { get; set; }

    public decimal? Direction { get; set; }

    public decimal? Altitude { get; set; }

    public string? LocationStatus { get; set; }

    public decimal? ContinueVoyage { get; set; }

    public decimal? TotalAverageFuel { get; set; }

    public string? Address { get; set; }

    public DateTime LocatedAtUtc { get; set; }

    public VehicleDeviceVendorType VendorType { get; set; }

    public string VendorDeviceId { get; set; } = default!;
}
