using System;
using System.Collections.Generic;

namespace CheMa.VNext.VehicleDevices;

public class VehicleDeviceStatusDto
{
    public Guid VehicleId { get; set; }

    public string Brand { get; set; } = default!;

    public string VendorDeviceId { get; set; } = default!;

    public VehicleDeviceBasicStatusDto Basic { get; set; } = new();

    public VehicleDeviceBodyStatusDto Body { get; set; } = new();

    public VehicleDeviceAlertStatusDto Alert { get; set; } = new();

    public DateTime StatusTimeUtc { get; set; }
}

public class VehicleDeviceBasicStatusDto
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

public class VehicleDeviceBodyStatusDto
{
    public bool? Locked { get; set; }

    public bool? LeftFrontDoorOpen { get; set; }

    public bool? RightFrontDoorOpen { get; set; }

    public bool? LeftRearDoorOpen { get; set; }

    public bool? RightRearDoorOpen { get; set; }

    public bool? TrunkOpen { get; set; }

    public bool? HoodOpen { get; set; }

    public bool? WindowOpen { get; set; }
}

public class VehicleDeviceAlertStatusDto
{
    public bool HasAlert { get; set; }

    public IReadOnlyList<VehicleDeviceAlertItemDto> Alerts { get; set; } = [];
}

public class VehicleDeviceAlertItemDto
{
    public string Code { get; set; } = default!;

    public string Message { get; set; } = default!;

    public VehicleDeviceAlertLevel Level { get; set; }

    public DateTime? AlertTimeUtc { get; set; }
}
