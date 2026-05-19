using System;
using Volo.Abp.Application.Dtos;

namespace CheMa.VNext.OpenPlatform.Dtos;

public class OpenAppVehicleAuthorizationDto : AuditedEntityDto<Guid>
{
    public Guid OpenAppId { get; set; }

    public Guid VehicleId { get; set; }

    public Guid VehicleDeviceId { get; set; }

    public string VehicleVin { get; set; } = string.Empty;

    public string DeviceVin { get; set; } = string.Empty;

    public string VendorDeviceId { get; set; } = string.Empty;

    public DateTime AuthorizationStartTime { get; set; }

    public DateTime? AuthorizationEndTime { get; set; }
}
