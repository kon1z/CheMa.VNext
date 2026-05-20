using System;
using System.Linq;
using System.Threading.Tasks;
using CheMa.VNext.Base;
using CheMa.VNext.OpenPlatform.AppServices;
using CheMa.VNext.OpenPlatform.Dtos;
using CheMa.VNext.OpenPlatform.Entities;
using CheMa.VNext.OpenPlatform.Inputs;
using CheMa.VNext.OpenPlatform.Repositories;
using CheMa.VNext.VehicleDevices;
using CheMa.VNext.VehicleDevices.Enums;
using CheMa.VNext.VehicleDevices.Interfaces;
using CheMa.VNext.VehicleDevices.Models;
using CheMa.VNext.VehicleDevices.Repositories;
using CheMa.VNext.Vehicles;
using CheMa.VNext.Vehicles.Entities;
using CheMa.VNext.Vehicles.Repositories;
using Volo.Abp;
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

    public async Task<OpenPlatformResponseDto<OpenPlatformAuthorizedDto>> GetAuthorizedAsync(GetVehicleControlAuthorizationInput input)
    {
        Check.NotNull(input, nameof(input));

        var context = await GetAuthorizedContextAsync(input.Vin);

        return Ok(new OpenPlatformAuthorizedDto
        {
            Vin = context.Authorization.VehicleVin,
            Authorized = true,
            AuthorizedTime = context.Authorization.AuthorizationStartTime,
            ExpireTime = context.Authorization.AuthorizationEndTime
        });
    }

    public async Task<OpenPlatformResponseDto<OpenPlatformVehicleInfoDto>> GetVehicleCurrentInfoAsync(GetVehicleControlAuthorizationInput input)
    {
        Check.NotNull(input, nameof(input));

        var context = await GetAuthorizedContextAsync(input.Vin);
        var vehicleDevice = await _vehicleDeviceRepository.FindByVehicleIdAsync(context.Vehicle.Id);
        if (vehicleDevice == null)
        {
            throw new BusinessException(VehicleDeviceErrorCodes.BindingNotFound)
                .WithData("VehicleId", context.Vehicle.Id);
        }

        var location = await _vehicleDeviceService.GetLocationAsync(context.Vehicle.Id);
        var status = await _vehicleDeviceService.GetStatusAsync(context.Vehicle.Id);

        return Ok(MapToVehicleCurrentInfo(context.Vehicle, location, status));
    }

    public async Task<OpenPlatformResponseDto<OpenPlatformTripListDto>> GetVehicleTripsAsync(GetOpenPlatformVehicleTripsInput input)
    {
        Check.NotNull(input, nameof(input));

        var context = await GetAuthorizedContextAsync(input.Vin);
        var result = await _vehicleDeviceService.GetTripsAsync(new VehicleDeviceTripQuery
        {
            VehicleId = context.Vehicle.Id,
            StartTimeUtc = input.StartTimeUtc,
            EndTimeUtc = input.EndTimeUtc
        });

        var trips = result.Trips
            .Where(x => !string.IsNullOrWhiteSpace(x.TripId))
            .ToList();
        var pageIndex = input.PageIndex <= 0 ? 1 : input.PageIndex;
        var pageSize = input.PageSize <= 0 ? 20 : input.PageSize;

        return Ok(new OpenPlatformTripListDto
        {
            TotalCount = trips.Count,
            Items = trips
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new OpenPlatformTripDto
                {
                    TripId = x.TripId,
                    StartTime = x.StartTimeUtc,
                    EndTime = x.EndTimeUtc,
                    StartAddress = x.StartAddress,
                    EndAddress = x.EndAddress,
                    Mileage = x.Mileage,
                    DurationSeconds = x.DurationSeconds,
                    AverageSpeed = x.AverageSpeed,
                    MaxSpeed = x.MaxSpeed
                })
                .ToList()
        });
    }

    public async Task<OpenPlatformResponseDto<OpenPlatformTripTrackDto>> GetVehicleTraceAsync(GetOpenPlatformVehicleTraceInput input)
    {
        Check.NotNull(input, nameof(input));
        Check.NotNullOrWhiteSpace(input.TripId, nameof(input.TripId));

        var context = await GetAuthorizedContextAsync(input.Vin);
        var result = await _vehicleDeviceService.GetTrackAsync(new VehicleDeviceTrackQuery
        {
            VehicleId = context.Vehicle.Id,
            TripId = input.TripId,
            StartTimeUtc = input.StartTimeUtc,
            EndTimeUtc = input.EndTimeUtc
        });

        return Ok(new OpenPlatformTripTrackDto
        {
            Vin = context.Vehicle.Vin,
            TripId = input.TripId,
            Points = result.Points.Select(x => new OpenPlatformTripTrackPointDto
            {
                Longitude = x.Longitude,
                Latitude = x.Latitude,
                Speed = x.Speed,
                Direction = x.Direction,
                GpsTime = x.LocatedAtUtc
            }).ToList()
        });
    }

    public async Task<OpenPlatformResponseDto<OpenPlatformControlResultDto>> ControlAsync(OpenPlatformControlInput input)
    {
        Check.NotNull(input, nameof(input));
        Check.NotNullOrWhiteSpace(input.Command, nameof(input.Command));

        var context = await GetAuthorizedContextAsync(input.Vin);
        var action = ParseControlAction(input.Command);
        var result = await _vehicleDeviceService.ControlAsync(new VehicleDeviceControlCommand
        {
            VehicleId = context.Vehicle.Id,
            Action = action
        });

        return Ok(new OpenPlatformControlResultDto
        {
            Success = result.Success,
            Vin = context.Vehicle.Vin,
            Command = input.Command,
            ExecuteTime = Clock.Now
        });
    }

    public async Task<OpenPlatformResponseDto<OpenPlatformAlarmListDto>> GetAlarmsAsync(GetOpenPlatformAlarmsInput input)
    {
        Check.NotNull(input, nameof(input));

        var context = await GetAuthorizedContextAsync(input.Vin);
        var result = await _vehicleDeviceService.GetAlertsAsync(new VehicleDeviceAlertQuery
        {
            VehicleId = context.Vehicle.Id,
            StartTimeUtc = input.StartTimeUtc,
            EndTimeUtc = input.EndTimeUtc
        });

        var alarms = result.Alerts.Select(x => new OpenPlatformAlarmDto
        {
            AlarmId = x.Code,
            AlarmType = x.Code,
            AlarmLevel = x.Level.ToString(),
            AlarmContent = x.Message,
            AlarmTime = x.AlertTimeUtc,
            Status = null
        }).ToList();

        return Ok(new OpenPlatformAlarmListDto
        {
            TotalCount = alarms.Count,
            Items = alarms
        });
    }

    public Task<OpenPlatformResponseDto<OpenPlatformCapabilityResultDto>> EnableStartAsync(OpenPlatformVehicleCapabilityInput input)
    {
        return SetStartCapabilityAsync(input, VehicleDeviceControlAction.EnableStart);
    }

    public Task<OpenPlatformResponseDto<OpenPlatformCapabilityResultDto>> DisableStartAsync(OpenPlatformVehicleCapabilityInput input)
    {
        return SetStartCapabilityAsync(input, VehicleDeviceControlAction.DisableStart);
    }

    private async Task<(OpenApp OpenApp, Vehicles.Entities.Vehicle Vehicle, VehicleControlAuthorization Authorization)> GetAuthorizedContextAsync(string vin)
    {
        Check.NotNullOrWhiteSpace(vin, nameof(vin), VehicleConsts.MaxVinLength);

        var clientId = _openPlatformRequestContextAccessor.Current?.ClientId;
        Check.NotNullOrWhiteSpace(clientId, nameof(OpenPlatformRequestContext.ClientId), OpenPlatformConsts.MaxClientIdLength);

        var openApp = await _openAppRepository.FirstOrDefaultAsync(x => x.ClientId == clientId);
        if (openApp == null)
        {
            throw new BusinessException("VNext:OpenPlatform:ClientIdNotFound")
                .WithData(nameof(clientId), clientId);
        }

        var vehicle = await _vehicleRepository.FirstOrDefaultAsync(x => x.Vin == vin);
        if (vehicle == null)
        {
            throw new BusinessException("VNext:OpenPlatform:VehicleNotFound")
                .WithData(nameof(vin), vin);
        }

        var authorization = await _vehicleControlAuthorizationRepository.FindCurrentByOpenAppIdAndVinAsync(openApp.Id, vin, Clock.Now);
        if (authorization == null)
        {
            throw new BusinessException("VNext:OpenPlatform:VehicleControlAuthorizationNotFound")
                .WithData(nameof(openApp.Id), openApp.Id)
                .WithData(nameof(vin), vin);
        }

        return (openApp, vehicle, authorization);
    }

    private async Task<OpenPlatformResponseDto<OpenPlatformCapabilityResultDto>> SetStartCapabilityAsync(
        OpenPlatformVehicleCapabilityInput input,
        VehicleDeviceControlAction action)
    {
        Check.NotNull(input, nameof(input));

        var context = await GetAuthorizedContextAsync(input.Vin);
        var result = await _vehicleDeviceService.ControlAsync(new VehicleDeviceControlCommand
        {
            VehicleId = context.Vehicle.Id,
            Action = action
        });

        return Ok(new OpenPlatformCapabilityResultDto
        {
            Success = result.Success,
            Vin = context.Vehicle.Vin,
            StartAllowed = action == VehicleDeviceControlAction.EnableStart,
            ExecuteTime = Clock.Now
        });
    }

    private static VehicleDeviceControlAction ParseControlAction(string command)
    {
        return command switch
        {
            "lock" => VehicleDeviceControlAction.Lock,
            "unlock" => VehicleDeviceControlAction.Unlock,
            "openTrunk" => VehicleDeviceControlAction.OpenTrunk,
            "findCar" => VehicleDeviceControlAction.FindCar,
            "horn" => VehicleDeviceControlAction.Honk,
            "flash" => VehicleDeviceControlAction.Flash,
            _ => throw new BusinessException(OpenPlatformResponseCodes.InvalidRequest)
                .WithData(nameof(command), command)
        };
    }

    private OpenPlatformResponseDto<T> Ok<T>(T data)
    {
        return new OpenPlatformResponseDto<T>
        {
            Data = data,
            TraceId = _openPlatformRequestContextAccessor.Current?.Nonce
        };
    }

    private static OpenPlatformVehicleInfoDto MapToVehicleCurrentInfo(
        Vehicle vehicle,
        VehicleDeviceLocationResult location,
        VehicleDeviceStatusResult status)
    {
        var updateTime = location.LocatedAtUtc != default ? location.LocatedAtUtc : status.StatusTimeUtc;

        return new OpenPlatformVehicleInfoDto
        {
            Vin = vehicle.Vin,
            PlateNo = vehicle.PlateNumber,
            Brand = vehicle.Brand,
            Series = vehicle.Series,
            Model = vehicle.Model,
            VehicleStatus = ToStatusCode(status.Basic.Online),
            LockStatus = ToStatusCode(status.Body.Locked),
            Mileage = status.Basic.Mileage,
            FuelPercent = status.Basic.FuelLevelPercent,
            SocPercent = status.Basic.BatteryLevelPercent,
            Longitude = location.Longitude,
            Latitude = location.Latitude,
            GpsTime = location.LocatedAtUtc == default ? null : location.LocatedAtUtc,
            UpdateTime = updateTime == default ? null : updateTime
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
