using System;
using System.Threading.Tasks;
using CheMa.VNext.Base;
using CheMa.VNext.OpenPlatform.AppServices;
using CheMa.VNext.OpenPlatform.Dtos;
using CheMa.VNext.OpenPlatform.Entities;
using CheMa.VNext.OpenPlatform.Inputs;
using CheMa.VNext.OpenPlatform.Repositories;
using CheMa.VNext.VehicleDevices;
using CheMa.VNext.VehicleDevices.Interfaces;
using CheMa.VNext.VehicleDevices.Models;
using CheMa.VNext.VehicleDevices.Repositories;
using CheMa.VNext.Vehicles;
using CheMa.VNext.Vehicles.Entities;
using CheMa.VNext.Vehicles.Repositories;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace CheMa.VNext.OpenPlatform.ApplicationServices;

public class OpenPlatformAppService : VNextAppService, IOpenPlatformAppService
{
    private readonly IRepository<OpenApp, Guid> _openAppRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IVehicleControlAuthorizationRepository _vehicleControlAuthorizationRepository;
    private readonly IVehicleDeviceRepository _vehicleDeviceRepository;
    private readonly IVehicleDeviceService _vehicleDeviceService;
    private readonly IOpenPlatformRequestContextAccessor _openPlatformRequestContextAccessor;

    public OpenPlatformAppService(
        IRepository<OpenApp, Guid> openAppRepository,
        IVehicleRepository vehicleRepository,
        IVehicleControlAuthorizationRepository vehicleControlAuthorizationRepository,
        IVehicleDeviceRepository vehicleDeviceRepository,
        IVehicleDeviceService vehicleDeviceService,
        IOpenPlatformRequestContextAccessor openPlatformRequestContextAccessor)
    {
        _openAppRepository = openAppRepository;
        _vehicleRepository = vehicleRepository;
        _vehicleControlAuthorizationRepository = vehicleControlAuthorizationRepository;
        _vehicleDeviceRepository = vehicleDeviceRepository;
        _vehicleDeviceService = vehicleDeviceService;
        _openPlatformRequestContextAccessor = openPlatformRequestContextAccessor;
    }

    public async Task<VehicleControlAuthorizationDto> GetAuthorizedAsync(GetVehicleControlAuthorizationInput input)
    {
        var context = await GetAuthorizedContextAsync(input);

        return new VehicleControlAuthorizationDto
        {
            OpenAppId = context.Authorization.OpenAppId,
            VehicleVin = context.Authorization.VehicleVin,
            DeviceVin = context.Authorization.DeviceVin,
            VendorDeviceId = context.Authorization.VendorDeviceId,
            AuthorizationStartTime = context.Authorization.AuthorizationStartTime,
            AuthorizationEndTime = context.Authorization.AuthorizationEndTime
        };
    }

    public async Task<OpenPlatformVehicleCurrentInfoDto> GetVehicleCurrentInfoAsync(GetVehicleControlAuthorizationInput input)
    {
        var context = await GetAuthorizedContextAsync(input);
        var vehicleDevice = await _vehicleDeviceRepository.FindByVehicleIdAsync(context.Vehicle.Id);
        if (vehicleDevice == null)
        {
            throw new BusinessException(VehicleDeviceErrorCodes.BindingNotFound)
                .WithData("VehicleId", context.Vehicle.Id);
        }

        var location = await _vehicleDeviceService.GetLocationAsync(context.Vehicle.Id);
        var status = await _vehicleDeviceService.GetStatusAsync(context.Vehicle.Id);

        return MapToVehicleCurrentInfo(context.Vehicle, location, status);
    }

    private async Task<(OpenApp OpenApp, Vehicles.Entities.Vehicle Vehicle, VehicleControlAuthorization Authorization)> GetAuthorizedContextAsync(GetVehicleControlAuthorizationInput input)
    {
        Check.NotNull(input, nameof(input));
        Check.NotNullOrWhiteSpace(input.Vin, nameof(input.Vin), VehicleConsts.MaxVinLength);

        var clientId = _openPlatformRequestContextAccessor.Current?.ClientId;
        Check.NotNullOrWhiteSpace(clientId, nameof(OpenPlatformRequestContext.ClientId), OpenPlatformConsts.MaxClientIdLength);

        var openApp = await _openAppRepository.FirstOrDefaultAsync(x => x.ClientId == clientId);
        if (openApp == null)
        {
            throw new BusinessException("VNext:OpenPlatform:ClientIdNotFound")
                .WithData(nameof(clientId), clientId);
        }

        var vehicle = await _vehicleRepository.FirstOrDefaultAsync(x => x.Vin == input.Vin);
        if (vehicle == null)
        {
            throw new BusinessException("VNext:OpenPlatform:VehicleNotFound")
                .WithData(nameof(input.Vin), input.Vin);
        }

        var authorization = await _vehicleControlAuthorizationRepository.FindCurrentByOpenAppIdAndVinAsync(openApp.Id, input.Vin, Clock.Now);
        if (authorization == null)
        {
            throw new BusinessException("VNext:OpenPlatform:VehicleControlAuthorizationNotFound")
                .WithData(nameof(openApp.Id), openApp.Id)
                .WithData(nameof(input.Vin), input.Vin);
        }

        return (openApp, vehicle, authorization);
    }

    private static OpenPlatformVehicleCurrentInfoDto MapToVehicleCurrentInfo(
        Vehicle vehicle,
        VehicleDeviceLocationResult location,
        VehicleDeviceStatusResult status)
    {
        return new OpenPlatformVehicleCurrentInfoDto
        {
            Vin = vehicle.Vin,
            Lat = location.Latitude,
            Lon = location.Longitude,
            Altitude = null,
            Speed = location.Speed ?? status.Basic.Speed,
            Direction = location.Direction,
            Door1Status = ToStatusCode(status.Body.LeftFrontDoorOpen),
            Door2Status = ToStatusCode(status.Body.RightFrontDoorOpen),
            Door3Status = ToStatusCode(status.Body.LeftRearDoorOpen),
            Door4Status = ToStatusCode(status.Body.RightRearDoorOpen),
            Door5Status = ToStatusCode(status.Body.TrunkOpen),
            LockStatus = ToStatusCode(status.Body.Locked),
            Engine = ToStatusCode(status.Basic.EngineOn),
            Bonnet = ToStatusCode(status.Body.HoodOpen),
            IgnitionStatus = ToStatusCode(status.Basic.EngineOn),
            OriginalMileage = status.Basic.Mileage,
            FuelLevel = status.Basic.FuelLevelPercent,
            FuelLevelUnit = status.Basic.FuelLevelPercent.HasValue ? "%" : null,
            TearDownAlarm = null,
            AccStatus = ToStatusCode(status.Basic.AccOn),
            LocationStatus = ToStatusCode(status.Basic.Online),
            ReportTime = location.LocatedAtUtc != default ? location.LocatedAtUtc : status.StatusTimeUtc,
            SendTime = null,
            ReceiveTime = null,
            Soc = status.Basic.BatteryLevelPercent,
            CarType = null,
            ContinueVoyage = null,
            ChargeStatus = null,
            ChargeType = null,
            CarVoltage = status.Basic.BatteryVoltage,
            LowTireTime = null,
            HighTemp = null,
            TireTempFl = null,
            TireTempFr = null,
            TireTempRl = null,
            TireTempRr = null,
            TireTempStatusFl = null,
            TireTempStatusFr = null,
            TireTempStatusRl = null,
            TireTempStatusRr = null,
            GearStatus = null,
            CarFortificationStatus = ToStatusCode(status.Body.DefendOn),
            BrakeStatus = null,
            AirConditionerStatus = null,
            FootBrakeStatus = null,
            Light1Status = null,
            Light2Status = null,
            Window1status = ToStatusCode(status.Body.WindowOpen),
            Window2status = ToStatusCode(status.Body.WindowOpen),
            Window3status = ToStatusCode(status.Body.WindowOpen),
            Window4status = ToStatusCode(status.Body.WindowOpen),
            Window5status = ToStatusCode(status.Body.WindowOpen),
            SeatVentilationStatus = null,
            SeatHeating = null,
            TotalAverageFuel = null
        };
    }

    private static int? ToStatusCode(bool? value)
    {
        return value switch
        {
            true => 1,
            false => 0,
            null => null
        };
    }
}
