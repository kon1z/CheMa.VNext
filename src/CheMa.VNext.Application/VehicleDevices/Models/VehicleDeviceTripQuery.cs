using System;

namespace CheMa.VNext.VehicleDevices.Models;

public class VehicleDeviceTripQuery
{
    public Guid VehicleId { get; set; }

    public DateTime StartTimeUtc { get; set; }

    public DateTime EndTimeUtc { get; set; }
}
