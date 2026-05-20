using System.ComponentModel.DataAnnotations;

namespace CheMa.VNext.OpenPlatform.Inputs;

public class OpenPlatformControlInput
{
    [Required]
    public string Vin { get; set; } = default!;

    [Required]
    public string Command { get; set; } = default!;
}
