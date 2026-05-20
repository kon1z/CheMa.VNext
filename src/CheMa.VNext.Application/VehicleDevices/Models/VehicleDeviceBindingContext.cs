namespace CheMa.VNext.VehicleDevices.Models;

public class VehicleDeviceBindingContext : VehicleDeviceContext
{
    public string EngineNumber { get; set; } = default!;

    public string? PlateNumber { get; set; }

    public string? BrandId { get; set; }

    public string? StyleId { get; set; }

    public string? ModelId { get; set; }
}
