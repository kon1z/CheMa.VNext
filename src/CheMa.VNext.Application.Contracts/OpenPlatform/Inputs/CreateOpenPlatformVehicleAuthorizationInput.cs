using System;

namespace CheMa.VNext.OpenPlatform.Inputs;

public class CreateOpenPlatformVehicleAuthorizationInput
{
    public string Vin { get; set; } = default!;

    public DateTime? AuthorizationStartTime { get; set; }

    public DateTime? AuthorizationEndTime { get; set; }
}
