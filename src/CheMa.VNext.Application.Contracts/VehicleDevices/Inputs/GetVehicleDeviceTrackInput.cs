using System;

namespace CheMa.VNext.VehicleDevices.Inputs;

public class GetVehicleDeviceTrackInput
{
    public Guid VehicleId { get; set; }

    public DateTime StartTimeUtc { get; set; }

    public DateTime EndTimeUtc { get; set; }
}
