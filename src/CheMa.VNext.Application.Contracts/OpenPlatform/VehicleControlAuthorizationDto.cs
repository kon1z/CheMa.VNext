using System;

namespace CheMa.VNext.OpenPlatform;

public class VehicleControlAuthorizationDto
{
    public Guid OpenAppId { get; set; }

    public string VehicleVin { get; set; } = default!;

    public string DeviceVin { get; set; } = default!;

    public string VendorDeviceId { get; set; } = default!;

    public DateTime AuthorizationStartTime { get; set; }

    public DateTime? AuthorizationEndTime { get; set; }
}
