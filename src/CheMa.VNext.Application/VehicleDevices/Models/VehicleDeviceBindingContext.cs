using System;
using CheMa.VNext.Vehicles.Enums;

namespace CheMa.VNext.VehicleDevices.Models;

public class VehicleDeviceBindingContext
{
    public Guid VehicleId { get; set; }

    public VehicleDeviceVendorType VendorType { get; set; }

    public string VendorDeviceId { get; set; } = default!;

    public string Vin { get; set; } = default!;
}
