using System;
using System.ComponentModel.DataAnnotations;

namespace CheMa.VNext.OpenPlatform.Inputs;

public class GetOpenPlatformAlarmsInput
{
    [Required]
    public string Vin { get; set; } = default!;

    public DateTime? StartTimeUtc { get; set; }

    public DateTime? EndTimeUtc { get; set; }
}
