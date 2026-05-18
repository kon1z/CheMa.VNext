using System;

namespace CheMa.VNext.VehicleDevices.Models;

public class BindVehicleDeviceResult
{
    public Guid VehicleId { get; set; }

    public string Brand { get; set; } = default!;

    public string VendorDeviceId { get; set; } = default!;

    public bool Success { get; set; }

    public bool AlreadyBound { get; set; }
}
