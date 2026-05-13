using System;

namespace CheMa.VNext.VehicleDevices;

public class VehicleDeviceTrackQuery
{
    public Guid VehicleId { get; set; }

    public DateTime StartTimeUtc { get; set; }

    public DateTime EndTimeUtc { get; set; }
}
