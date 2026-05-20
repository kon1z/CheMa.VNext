using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CheMa.VNext.VehicleDevices.Dtos;
using CheMa.VNext.VehicleDevices.Interfaces;
using CheMa.VNext.VehicleDevices.Managers;
using CheMa.VNext.VehicleDevices.Models;
using CheMa.VNext.Vehicles.Entities;
using CheMa.VNext.Vehicles.Repositories;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace CheMa.VNext.VehicleCapabilities.Shared;

public class VehicleCapabilityOrchestrator : IVehicleCapabilityOrchestrator, ITransientDependency
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IVehicleDeviceService _vehicleDeviceService;
    private readonly VehicleTelematicsManager _vehicleTelematicsManager;
    private readonly IVehicleCapabilityAuthorizer _vehicleCapabilityAuthorizer;
    private readonly ILogger<VehicleCapabilityOrchestrator> _logger;

    public VehicleCapabilityOrchestrator(
        IVehicleRepository vehicleRepository,
        IVehicleDeviceService vehicleDeviceService,
        VehicleTelematicsManager vehicleTelematicsManager,
        IVehicleCapabilityAuthorizer vehicleCapabilityAuthorizer,
        ILogger<VehicleCapabilityOrchestrator> logger)
    {
        _vehicleRepository = vehicleRepository;
        _vehicleDeviceService = vehicleDeviceService;
        _vehicleTelematicsManager = vehicleTelematicsManager;
        _vehicleCapabilityAuthorizer = vehicleCapabilityAuthorizer;
        _logger = logger;
    }

    public async Task<VehicleCapabilityResultDto> GetVehicleInfoAsync(VehicleAccessContext context, Guid vehicleId)
    {
        var result = await CreateBaseResultAsync(context, vehicleId, VehicleCapabilityCodes.VehicleInfoQuery);
        result.Location = MapLocation(await _vehicleDeviceService.GetLocationAsync(vehicleId));
        result.Status = MapStatus(await _vehicleDeviceService.GetStatusAsync(vehicleId));
        return result;
    }

    public async Task<VehicleCapabilityResultDto> GetVehicleStatusAsync(VehicleAccessContext context, Guid vehicleId)
    {
        var result = await CreateBaseResultAsync(context, vehicleId, VehicleCapabilityCodes.VehicleStatusQuery);
        result.Status = MapStatus(await _vehicleDeviceService.GetStatusAsync(vehicleId));
        return result;
    }

    public async Task<VehicleCapabilityResultDto> GetVehicleLocationAsync(VehicleAccessContext context, Guid vehicleId)
    {
        var result = await CreateBaseResultAsync(context, vehicleId, VehicleCapabilityCodes.VehicleLocationQuery);
        result.Location = MapLocation(await _vehicleDeviceService.GetLocationAsync(vehicleId));
        return result;
    }

    public async Task<VehicleDeviceTripResult> GetVehicleTripsAsync(
        VehicleAccessContext context,
        VehicleDeviceTripQuery query)
    {
        await EnsureQueryCapabilityAsync(context, query.VehicleId, VehicleCapabilityCodes.VehicleTripsQuery);
        return await _vehicleDeviceService.GetTripsAsync(query);
    }

    public async Task<VehicleDeviceTrackResult> GetVehicleTrackAsync(
        VehicleAccessContext context,
        VehicleDeviceTrackQuery query)
    {
        await EnsureQueryCapabilityAsync(context, query.VehicleId, VehicleCapabilityCodes.VehicleTrackQuery);
        return await _vehicleDeviceService.GetTrackAsync(query);
    }

    public async Task<VehicleDeviceAlertResult> GetVehicleAlertsAsync(
        VehicleAccessContext context,
        VehicleDeviceAlertQuery query)
    {
        await EnsureQueryCapabilityAsync(context, query.VehicleId, VehicleCapabilityCodes.VehicleAlertsQuery);
        return await _vehicleDeviceService.GetAlertsAsync(query);
    }

    private async Task<VehicleCapabilityResultDto> CreateBaseResultAsync(
        VehicleAccessContext context,
        Guid vehicleId,
        string capabilityCode)
    {
        var stopwatch = Stopwatch.StartNew();
        var vehicle = await _vehicleRepository.GetAsync(vehicleId);
        var vehicleDevice = await _vehicleTelematicsManager.GetBoundDeviceAsync(vehicle);

        await _vehicleCapabilityAuthorizer.AuthorizeAsync(context, new VehicleCapabilityRequirement
        {
            VehicleId = vehicleId,
            CapabilityCode = capabilityCode,
            OperationType = VehicleCapabilityOperationType.Query
        });

        _vehicleTelematicsManager.EnsureCanUseQueryCapability(vehicle, vehicleDevice, capabilityCode);
        stopwatch.Stop();

        _logger.LogInformation(
            "Vehicle capability {CapabilityCode} authorized for vehicle {VehicleId} from channel {AccessChannel} in {ElapsedMs} ms",
            capabilityCode,
            vehicleId,
            context.Channel,
            stopwatch.ElapsedMilliseconds);

        return MapBase(vehicle);
    }

    private async Task EnsureQueryCapabilityAsync(
        VehicleAccessContext context,
        Guid vehicleId,
        string capabilityCode)
    {
        var stopwatch = Stopwatch.StartNew();
        var vehicle = await _vehicleRepository.GetAsync(vehicleId);
        var vehicleDevice = await _vehicleTelematicsManager.GetBoundDeviceAsync(vehicle);

        await _vehicleCapabilityAuthorizer.AuthorizeAsync(context, new VehicleCapabilityRequirement
        {
            VehicleId = vehicleId,
            CapabilityCode = capabilityCode,
            OperationType = VehicleCapabilityOperationType.Query
        });

        _vehicleTelematicsManager.EnsureCanUseQueryCapability(vehicle, vehicleDevice, capabilityCode);
        stopwatch.Stop();

        _logger.LogInformation(
            "Vehicle capability {CapabilityCode} authorized for vehicle {VehicleId} from channel {AccessChannel} in {ElapsedMs} ms",
            capabilityCode,
            vehicleId,
            context.Channel,
            stopwatch.ElapsedMilliseconds);
    }

    private static VehicleCapabilityResultDto MapBase(Vehicle vehicle)
    {
        return new VehicleCapabilityResultDto
        {
            VehicleId = vehicle.Id,
            Vin = vehicle.Vin,
            PlateNumber = vehicle.PlateNumber,
            Brand = vehicle.Brand,
            Series = vehicle.Series,
            Model = vehicle.Model
        };
    }

    private static VehicleDeviceLocationDto MapLocation(VehicleDeviceLocationResult result)
    {
        return new VehicleDeviceLocationDto
        {
            VehicleId = result.VehicleId,
            Longitude = result.Longitude,
            Latitude = result.Latitude,
            CoordinateSystem = result.CoordinateSystem,
            Speed = result.Speed,
            Direction = result.Direction,
            Address = result.Address,
            LocatedAtUtc = result.LocatedAtUtc,
            VendorType = result.VendorType,
            VendorDeviceId = result.VendorDeviceId
        };
    }

    private static VehicleDeviceStatusDto MapStatus(VehicleDeviceStatusResult result)
    {
        return new VehicleDeviceStatusDto
        {
            VehicleId = result.VehicleId,
            VendorType = result.VendorType,
            VendorDeviceId = result.VendorDeviceId,
            StatusTimeUtc = result.StatusTimeUtc,
            Basic = new VehicleDeviceBasicStatusDto
            {
                Online = result.Basic.Online,
                AccOn = result.Basic.AccOn,
                EngineOn = result.Basic.EngineOn,
                Speed = result.Basic.Speed,
                Mileage = result.Basic.Mileage,
                FuelLevelPercent = result.Basic.FuelLevelPercent,
                BatteryLevelPercent = result.Basic.BatteryLevelPercent,
                BatteryVoltage = result.Basic.BatteryVoltage
            },
            Body = new VehicleDeviceBodyStatusDto
            {
                Locked = result.Body.Locked,
                LeftFrontDoorOpen = result.Body.LeftFrontDoorOpen,
                RightFrontDoorOpen = result.Body.RightFrontDoorOpen,
                LeftRearDoorOpen = result.Body.LeftRearDoorOpen,
                RightRearDoorOpen = result.Body.RightRearDoorOpen,
                TrunkOpen = result.Body.TrunkOpen,
                HoodOpen = result.Body.HoodOpen,
                WindowOpen = result.Body.WindowOpen,
                DefendOn = result.Body.DefendOn
            },
            Alert = new VehicleDeviceAlertStatusDto
            {
                HasAlert = result.Alert.HasAlert,
                Alerts = result.Alert.Alerts.Select(x => new VehicleDeviceAlertItemDto
                {
                    Code = x.Code,
                    Message = x.Message,
                    Level = x.Level,
                    AlertTimeUtc = x.AlertTimeUtc
                }).ToList()
            }
        };
    }
}
