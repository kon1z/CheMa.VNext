using System.Collections.Generic;

namespace CheMa.VNext.OpenPlatform.Dtos;

public class OpenPlatformVehicleTripsDto
{
    public string Vin { get; set; } = default!;

    public IReadOnlyList<OpenPlatformVehicleTripDto> Trips { get; set; } = [];
}

public class OpenPlatformVehicleTripDto
{
    public string TripId { get; set; } = default!;
}
