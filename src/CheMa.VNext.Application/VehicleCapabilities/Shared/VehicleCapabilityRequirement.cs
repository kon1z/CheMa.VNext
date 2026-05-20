using System;

namespace CheMa.VNext.VehicleCapabilities.Shared;

public class VehicleCapabilityRequirement
{
    public Guid VehicleId { get; set; }

    public string CapabilityCode { get; set; } = default!;

    public VehicleCapabilityOperationType OperationType { get; set; }
}
