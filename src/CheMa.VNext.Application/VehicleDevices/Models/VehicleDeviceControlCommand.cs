using System;

namespace CheMa.VNext.VehicleDevices.Models;

public class VehicleDeviceControlCommand
{
    public Guid VehicleId { get; set; }

    public VehicleDeviceControlAction Action { get; set; }
}
