using System;

namespace CheMa.VNext.VehicleDevices.Models;

public class VehicleDeviceContext
{
    public Guid VehicleId { get; set; }

    public string Brand { get; set; } = default!;

    public string VendorDeviceId { get; set; } = default!;

    public string Vin { get; set; } = default!;
}
