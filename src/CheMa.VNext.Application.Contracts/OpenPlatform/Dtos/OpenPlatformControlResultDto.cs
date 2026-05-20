using System;

namespace CheMa.VNext.OpenPlatform.Dtos;

public class OpenPlatformControlResultDto
{
    public bool Success { get; set; }

    public string Vin { get; set; } = default!;

    public string Command { get; set; } = default!;

    public DateTime ExecuteTime { get; set; }
}
