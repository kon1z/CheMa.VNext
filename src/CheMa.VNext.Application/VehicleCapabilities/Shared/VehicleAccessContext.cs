using System;

namespace CheMa.VNext.VehicleCapabilities.Shared;

public class VehicleAccessContext
{
    public VehicleAccessChannel Channel { get; set; }

    public Guid? UserId { get; set; }

    public string? ClientId { get; set; }

    public Guid? OpenAppId { get; set; }

    public Guid? TenantId { get; set; }

    public bool IsAdmin { get; set; }
}
