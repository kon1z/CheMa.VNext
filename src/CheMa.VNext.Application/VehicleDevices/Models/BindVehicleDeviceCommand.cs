using System;

using CheMa.VNext.Vehicles;

namespace CheMa.VNext.VehicleDevices.Models;

public class BindVehicleDeviceCommand
{
    public Guid VehicleId { get; set; }

    public VehicleDeviceVendorType VendorType { get; set; }

    public string VendorDeviceId { get; set; } = default!;
}
