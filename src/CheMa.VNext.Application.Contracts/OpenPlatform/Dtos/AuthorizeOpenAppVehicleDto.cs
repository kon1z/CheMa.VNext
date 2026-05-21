using System;

namespace CheMa.VNext.OpenPlatform.Dtos;

public class AuthorizeOpenAppVehicleDto
{
    public Guid VehicleId { get; set; }

    public DateTime AuthorizationStartTime { get; set; }

    public DateTime? AuthorizationEndTime { get; set; }
}
