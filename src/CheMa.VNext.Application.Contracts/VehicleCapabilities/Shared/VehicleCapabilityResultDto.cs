using System;
using CheMa.VNext.VehicleDevices.Dtos;

namespace CheMa.VNext.VehicleCapabilities.Shared;

public class VehicleCapabilityResultDto
{
    public Guid VehicleId { get; set; }

    public string Vin { get; set; } = default!;

    public string? PlateNumber { get; set; }

    public string? Brand { get; set; }

    public string? Series { get; set; }

    public string? Model { get; set; }

    public VehicleDeviceLocationDto? Location { get; set; }

    public VehicleDeviceStatusDto? Status { get; set; }
}
