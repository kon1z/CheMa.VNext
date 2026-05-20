using System;
using System.Collections.Generic;
using CheMa.VNext.VehicleDevices.Enums;
using CheMa.VNext.Vehicles.Enums;

namespace CheMa.VNext.VehicleDevices.Models;

public class VehicleDeviceStatusResult
{
    public Guid VehicleId { get; set; }

    public VehicleDeviceVendorType VendorType { get; set; }

    public string VendorDeviceId { get; set; } = default!;

    public VehicleDeviceBasicStatus Basic { get; set; } = new();

    public VehicleDeviceBodyStatus Body { get; set; } = new();

    public VehicleDeviceAlertStatus Alert { get; set; } = new();

    public DateTime StatusTimeUtc { get; set; }
}

public class VehicleDeviceBasicStatus
{
    public bool? Online { get; set; }

    public bool? AccOn { get; set; }

    public bool? EngineOn { get; set; }

    public decimal? Speed { get; set; }

    public decimal? Mileage { get; set; }

    public decimal? FuelLevelPercent { get; set; }

    public decimal? BatteryLevelPercent { get; set; }

    public decimal? BatteryVoltage { get; set; }
}

public class VehicleDeviceBodyStatus
{
    public bool? Locked { get; set; }

    public bool? LeftFrontDoorOpen { get; set; }

    public bool? RightFrontDoorOpen { get; set; }

    public bool? LeftRearDoorOpen { get; set; }

    public bool? RightRearDoorOpen { get; set; }

    public bool? TrunkOpen { get; set; }

    public bool? HoodOpen { get; set; }

    public bool? WindowOpen { get; set; }

    public bool? DefendOn { get; set; }
}

public class VehicleDeviceAlertStatus
{
    public bool HasAlert { get; set; }

    public IReadOnlyList<VehicleDeviceAlertItem> Alerts { get; set; } = [];
}

public class VehicleDeviceAlertItem
{
    public string Code { get; set; } = default!;

    public string Message { get; set; } = default!;

    public VehicleDeviceAlertLevel Level { get; set; }

    public DateTime? AlertTimeUtc { get; set; }
}
