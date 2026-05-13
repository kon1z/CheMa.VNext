using System;

namespace CheMa.VNext.VehicleDevices;

public class UnbindVehicleDeviceResult
{
    public Guid VehicleId { get; set; }

    public string Brand { get; set; } = default!;

    public string VendorDeviceId { get; set; } = default!;

    public bool Success { get; set; }
}
