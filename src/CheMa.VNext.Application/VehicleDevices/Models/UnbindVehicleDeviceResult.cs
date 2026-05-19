using System;
using CheMa.VNext.Vehicles;

namespace CheMa.VNext.VehicleDevices.Models;

public class UnbindVehicleDeviceResult
{
    public Guid VehicleId { get; set; }

    public VehicleDeviceVendorType VendorType { get; set; }

    public string VendorDeviceId { get; set; } = default!;

    public bool Success { get; set; }
}
