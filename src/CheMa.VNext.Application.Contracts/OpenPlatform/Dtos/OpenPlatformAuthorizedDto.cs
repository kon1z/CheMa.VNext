using System;

namespace CheMa.VNext.OpenPlatform.Dtos;

public class OpenPlatformAuthorizedDto
{
    public string Vin { get; set; } = default!;

    public bool Authorized { get; set; }

    public DateTime AuthorizedTime { get; set; }

    public DateTime? ExpireTime { get; set; }
}
