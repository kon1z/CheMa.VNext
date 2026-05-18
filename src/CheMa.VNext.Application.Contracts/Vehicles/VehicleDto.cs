using System;
using Volo.Abp.Application.Dtos;

namespace CheMa.VNext.Vehicles;

public class VehicleDto : FullAuditedEntityDto<Guid>
{
    public string Vin { get; set; } = default!;

    public string? PlateNumber { get; set; }

    public VehicleDeviceType DeviceType { get; set; }

    public VehicleBindingStatus BindingStatus { get; set; }

    public DateTime? BindingTime { get; set; }
}
