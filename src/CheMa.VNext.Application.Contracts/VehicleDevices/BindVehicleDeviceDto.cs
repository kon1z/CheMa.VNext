using System;
using CheMa.VNext.Vehicles;

namespace CheMa.VNext.VehicleDevices;

public class BindVehicleDeviceDto
{
    public Guid VehicleId { get; set; }

    public VehicleDeviceVendorType VendorType { get; set; }

    public string VendorDeviceId { get; set; } = default!;

    public bool Success { get; set; }

    public bool AlreadyBound { get; set; }
}
