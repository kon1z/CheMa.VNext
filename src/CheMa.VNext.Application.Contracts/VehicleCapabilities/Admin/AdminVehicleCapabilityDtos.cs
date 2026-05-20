using System;
using System.Collections.Generic;

namespace CheMa.VNext.VehicleCapabilities.Admin;

public class AdminVehicleTripQueryDto
{
    public Guid VehicleId { get; set; }

    public DateTime StartTimeUtc { get; set; }

    public DateTime EndTimeUtc { get; set; }

    public int PageIndex { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}

public class AdminVehicleTrackQueryDto
{
    public Guid VehicleId { get; set; }

    public string? TripId { get; set; }

    public DateTime StartTimeUtc { get; set; }

    public DateTime EndTimeUtc { get; set; }
}

public class AdminVehicleAlertQueryDto
{
    public Guid VehicleId { get; set; }

    public DateTime? StartTimeUtc { get; set; }

    public DateTime? EndTimeUtc { get; set; }
}

public class AdminVehicleTripListDto
{
    public int TotalCount { get; set; }

    public IReadOnlyList<AdminVehicleTripDto> Items { get; set; } = [];
}

public class AdminVehicleTripDto
{
    public string TripId { get; set; } = default!;

    public DateTime? StartTimeUtc { get; set; }

    public DateTime? EndTimeUtc { get; set; }

    public string? StartAddress { get; set; }

    public string? EndAddress { get; set; }

    public decimal? Mileage { get; set; }

    public int? DurationSeconds { get; set; }

    public decimal? AverageSpeed { get; set; }

    public decimal? MaxSpeed { get; set; }
}

public class AdminVehicleTrackDto
{
    public Guid VehicleId { get; set; }

    public string? TripId { get; set; }

    public IReadOnlyList<AdminVehicleTrackPointDto> Points { get; set; } = [];
}

public class AdminVehicleTrackPointDto
{
    public decimal Longitude { get; set; }

    public decimal Latitude { get; set; }

    public decimal? Speed { get; set; }

    public decimal? Direction { get; set; }

    public DateTime LocatedAtUtc { get; set; }
}

public class AdminVehicleAlertListDto
{
    public int TotalCount { get; set; }

    public IReadOnlyList<AdminVehicleAlertDto> Items { get; set; } = [];
}

public class AdminVehicleAlertDto
{
    public string Code { get; set; } = default!;

    public string Message { get; set; } = default!;

    public string Level { get; set; } = default!;

    public DateTime? AlertTimeUtc { get; set; }
}
