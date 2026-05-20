using System;

namespace CheMa.VNext.OpenPlatform.Dtos;

public class OpenPlatformCapabilityResultDto
{
    public bool Success { get; set; }

    public string Vin { get; set; } = default!;

    public bool StartAllowed { get; set; }

    public DateTime ExecuteTime { get; set; }
}
