using System;
using System.Collections.Generic;

namespace CheMa.VNext.VehicleDevices.Models;

public class VehicleDeviceAlertResult
{
    public Guid VehicleId { get; set; }

    public IReadOnlyList<VehicleDeviceAlertItem> Alerts { get; set; } = [];
}

public class VehicleDeviceAlertQuery
{
    public Guid VehicleId { get; set; }

    public DateTime? StartTimeUtc { get; set; }

    public DateTime? EndTimeUtc { get; set; }
}
