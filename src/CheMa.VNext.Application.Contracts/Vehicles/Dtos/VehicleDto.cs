using System;
using CheMa.VNext.Vehicles.Enums;
using Volo.Abp.Application.Dtos;

namespace CheMa.VNext.Vehicles.Dtos;

public class VehicleDto : FullAuditedEntityDto<Guid>
{
    public string Vin { get; set; } = default!;

    public string? PlateNumber { get; set; }

    public string? Brand { get; set; }

    public string? Series { get; set; }

    public string? Model { get; set; }

    public VehicleDeviceVendorType? VendorType { get; set; }

    public VehicleBindingStatus BindingStatus { get; set; }

    public DateTime? BindingTime { get; set; }
}
