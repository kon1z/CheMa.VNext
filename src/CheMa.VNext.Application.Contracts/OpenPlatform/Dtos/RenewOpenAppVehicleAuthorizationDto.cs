using System;

namespace CheMa.VNext.OpenPlatform.Dtos;

public class RenewOpenAppVehicleAuthorizationDto
{
    public DateTime AuthorizationStartTime { get; set; }

    public DateTime? AuthorizationEndTime { get; set; }
}
