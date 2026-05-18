using System;

namespace CheMa.VNext.VehicleDevices.Models;

public class VehicleDeviceControlResult
{
    public Guid VehicleId { get; set; }

    public VehicleDeviceControlAction Action { get; set; }

    public bool Success { get; set; }

    public string? Message { get; set; }

    public string Brand { get; set; } = default!;

    public string VendorDeviceId { get; set; } = default!;
}
