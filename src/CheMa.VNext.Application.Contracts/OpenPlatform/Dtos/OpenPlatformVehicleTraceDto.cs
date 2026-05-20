using System;
using System.Collections.Generic;

namespace CheMa.VNext.OpenPlatform.Dtos;

public class OpenPlatformVehicleTraceDto
{
    public string Vin { get; set; } = default!;

    public string TripId { get; set; } = default!;

    public IReadOnlyList<OpenPlatformVehicleTracePointDto> Points { get; set; } = [];
}

public class OpenPlatformVehicleTracePointDto
{
    public decimal Lon { get; set; }

    public decimal Lat { get; set; }

    public DateTime LocatedAtUtc { get; set; }
}
