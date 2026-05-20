using System;
using System.Collections.Generic;

namespace CheMa.VNext.OpenPlatform.Dtos;

public class OpenPlatformTripTrackDto
{
    public string Vin { get; set; } = default!;

    public string TripId { get; set; } = default!;

    public IReadOnlyList<OpenPlatformTripTrackPointDto> Points { get; set; } = [];
}

public class OpenPlatformTripTrackPointDto
{
    public decimal Longitude { get; set; }

    public decimal Latitude { get; set; }

    public decimal? Speed { get; set; }

    public decimal? Direction { get; set; }

    public DateTime GpsTime { get; set; }
}
