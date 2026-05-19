using System;
using CheMa.VNext.Vehicles;

namespace CheMa.VNext.VehicleDevices.Inputs;

public class BindVehicleDeviceInput
{
    public Guid VehicleId { get; set; }

    public VehicleDeviceVendorType VendorType { get; set; }

    public string VendorDeviceId { get; set; } = default!;
}
