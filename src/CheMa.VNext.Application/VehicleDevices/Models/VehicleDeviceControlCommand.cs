using System;

namespace CheMa.VNext.VehicleDevices;

public class VehicleDeviceControlCommand
{
    public Guid VehicleId { get; set; }

    public VehicleDeviceControlAction Action { get; set; }
}
