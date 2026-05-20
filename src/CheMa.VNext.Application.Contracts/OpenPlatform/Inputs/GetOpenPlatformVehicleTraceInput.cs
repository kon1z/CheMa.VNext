using System;

namespace CheMa.VNext.OpenPlatform.Inputs;

public class GetOpenPlatformVehicleTraceInput
{
    public string Vin { get; set; } = default!;

    public string TripId { get; set; } = default!;

    public DateTime StartTimeUtc { get; set; }

    public DateTime EndTimeUtc { get; set; }
}
