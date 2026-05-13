using System;
using System.Collections.Generic;

namespace CheMa.VNext.VehicleDevices;

public class VehicleDeviceTrackResult
{
    public Guid VehicleId { get; set; }

    public string CoordinateSystem { get; set; } = VehicleDeviceConsts.CoordinateSystemBd09;

    public IReadOnlyList<VehicleDeviceTrackPoint> Points { get; set; } = [];
}

public class VehicleDeviceTrackPoint
{
    public decimal Longitude { get; set; }

    public decimal Latitude { get; set; }

    public decimal? Speed { get; set; }

    public decimal? Direction { get; set; }

    public decimal? Mileage { get; set; }

    public DateTime LocatedAtUtc { get; set; }
}
