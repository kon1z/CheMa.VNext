using System;
using System.Collections.Generic;

namespace CheMa.VNext.VehicleDevices.Models;

public class VehicleDeviceTripResult
{
    public Guid VehicleId { get; set; }

    public IReadOnlyList<VehicleDeviceTrip> Trips { get; set; } = [];
}

public class VehicleDeviceTrip
{
    public string TripId { get; set; } = default!;

    public DateTime? StartTimeUtc { get; set; }

    public DateTime? EndTimeUtc { get; set; }

    public string? StartAddress { get; set; }

    public string? EndAddress { get; set; }

    public decimal? Mileage { get; set; }

    public int? DurationSeconds { get; set; }

    public decimal? AverageSpeed { get; set; }

    public decimal? MaxSpeed { get; set; }
}
