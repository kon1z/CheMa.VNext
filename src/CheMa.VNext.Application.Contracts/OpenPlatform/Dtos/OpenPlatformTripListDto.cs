using System;
using System.Collections.Generic;

namespace CheMa.VNext.OpenPlatform.Dtos;

public class OpenPlatformTripListDto
{
    public int TotalCount { get; set; }

    public IReadOnlyList<OpenPlatformTripDto> Items { get; set; } = [];
}

public class OpenPlatformTripDto
{
    public string TripId { get; set; } = default!;

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string? StartAddress { get; set; }

    public string? EndAddress { get; set; }

    public decimal? Mileage { get; set; }

    public int? DurationSeconds { get; set; }

    public decimal? AverageSpeed { get; set; }

    public decimal? MaxSpeed { get; set; }
}
