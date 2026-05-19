using System;
using CheMa.VNext.Vehicles;

namespace CheMa.VNext.VehicleDevices;

public class VehicleDeviceControlDto
{
    public Guid VehicleId { get; set; }

    public VehicleDeviceControlAction Action { get; set; }

    public bool Success { get; set; }

    public string? Message { get; set; }

    public VehicleDeviceVendorType VendorType { get; set; }

    public string VendorDeviceId { get; set; } = default!;
}
