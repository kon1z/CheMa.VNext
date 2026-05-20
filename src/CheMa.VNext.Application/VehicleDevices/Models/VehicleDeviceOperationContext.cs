namespace CheMa.VNext.VehicleDevices.Models;

public class VehicleDeviceOperationContext : VehicleDeviceContext
{
    public string VendorVehicleId { get; set; } = default!;

    public string VendorVehicleHwId { get; set; } = default!;
}
