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
}
