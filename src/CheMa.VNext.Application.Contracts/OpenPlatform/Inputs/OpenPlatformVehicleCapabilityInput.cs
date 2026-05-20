using System.ComponentModel.DataAnnotations;

namespace CheMa.VNext.OpenPlatform.Inputs;

public class OpenPlatformVehicleCapabilityInput
{
    [Required]
    public string Vin { get; set; } = default!;
}
