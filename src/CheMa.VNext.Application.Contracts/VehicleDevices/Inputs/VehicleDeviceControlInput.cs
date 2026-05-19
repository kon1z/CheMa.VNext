using System;

namespace CheMa.VNext.VehicleDevices.Inputs;

public class VehicleDeviceControlInput
{
    public Guid VehicleId { get; set; }

    public VehicleDeviceControlAction Action { get; set; }
}
