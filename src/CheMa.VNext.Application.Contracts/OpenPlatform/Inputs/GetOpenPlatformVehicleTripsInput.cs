using System;

namespace CheMa.VNext.OpenPlatform.Inputs;

public class GetOpenPlatformVehicleTripsInput
{
    public string Vin { get; set; } = default!;

    public DateTime StartTimeUtc { get; set; }

    public DateTime EndTimeUtc { get; set; }
}
