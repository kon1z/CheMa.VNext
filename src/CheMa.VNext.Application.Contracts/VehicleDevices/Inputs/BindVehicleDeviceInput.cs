using System;
using CheMa.VNext.Vehicles.Enums;

namespace CheMa.VNext.VehicleDevices.Inputs;

public class BindVehicleDeviceInput
{
    public Guid VehicleId { get; set; }

    public VehicleDeviceVendorType VendorType { get; set; }

    public string VendorDeviceId { get; set; } = default!;

    public string? BrandId { get; set; }

    public string? StyleId { get; set; }

    public string? ModelId { get; set; }
}
