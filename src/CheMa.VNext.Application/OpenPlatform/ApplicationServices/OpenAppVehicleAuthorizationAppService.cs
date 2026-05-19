using System;
using System.Threading.Tasks;
using CheMa.VNext.Base;
using CheMa.VNext.OpenPlatform.AppServices;
using CheMa.VNext.OpenPlatform.Dtos;
using CheMa.VNext.OpenPlatform.Entities;
using CheMa.VNext.OpenPlatform.Inputs;
using CheMa.VNext.OpenPlatform.Repositories;
using CheMa.VNext.VehicleDevices.Entities;
using CheMa.VNext.VehicleDevices.Repositories;
using CheMa.VNext.Vehicles.Entities;
using CheMa.VNext.Vehicles.Repositories;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace CheMa.VNext.OpenPlatform.ApplicationServices;

public class OpenAppVehicleAuthorizationAppService : VNextAppService, IOpenAppVehicleAuthorizationAppService
{
    private readonly IRepository<OpenApp, Guid> _openAppRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IVehicleDeviceRepository _vehicleDeviceRepository;
    private readonly IVehicleControlAuthorizationRepository _vehicleControlAuthorizationRepository;

    public OpenAppVehicleAuthorizationAppService(
        IRepository<OpenApp, Guid> openAppRepository,
        IVehicleRepository vehicleRepository,
        IVehicleDeviceRepository vehicleDeviceRepository,
        IVehicleControlAuthorizationRepository vehicleControlAuthorizationRepository)
    {
        _openAppRepository = openAppRepository;
        _vehicleRepository = vehicleRepository;
        _vehicleDeviceRepository = vehicleDeviceRepository;
        _vehicleControlAuthorizationRepository = vehicleControlAuthorizationRepository;
    }

    public async Task<OpenAppVehicleAuthorizationDto> AuthorizeAsync(Guid openAppId, AuthorizeOpenAppVehicleDto input)
    {
        await CheckManagePolicyAsync();
        Check.NotNull(input, nameof(input));

        var openApp = await GetOpenAppAsync(openAppId);
        ValidateAuthorizationPeriod(input.AuthorizationStartTime, input.AuthorizationEndTime);
        ValidateOpenAppAvailability(openApp, input.AuthorizationStartTime, input.AuthorizationEndTime);

        var vehicle = await GetVehicleAsync(input.VehicleId);
        var vehicleDevice = await GetCurrentVehicleDeviceAsync(vehicle.Id);

        var conflictAuthorization = await _vehicleControlAuthorizationRepository.FindConflictAsync(
            vehicle.Id,
            input.AuthorizationStartTime,
            input.AuthorizationEndTime);

        if (conflictAuthorization != null && conflictAuthorization.OpenAppId != openApp.Id)
        {
            throw new BusinessException("VNext:OpenPlatform:VehicleControlAuthorizationConflict")
                .WithData(nameof(openAppId), openAppId)
                .WithData(nameof(input.VehicleId), input.VehicleId)
                .WithData(nameof(input.AuthorizationStartTime), input.AuthorizationStartTime)
                .WithData(nameof(input.AuthorizationEndTime), input.AuthorizationEndTime);
        }

        var authorization = await _vehicleControlAuthorizationRepository.FindByOpenAppIdAndVehicleIdAsync(openApp.Id, vehicle.Id);
        if (authorization == null)
        {
            authorization = new VehicleControlAuthorization(
                GuidGenerator.Create(),
                openApp.Id,
                vehicle.Id,
                vehicleDevice.Id,
                vehicle.Vin,
                vehicle.Vin,
                vehicleDevice.VendorDeviceId,
                input.AuthorizationStartTime,
                input.AuthorizationEndTime);

            await _vehicleControlAuthorizationRepository.InsertAsync(authorization, autoSave: true);
        }
        else
        {
            authorization.Renew(
                vehicleDevice.Id,
                vehicle.Vin,
                vehicle.Vin,
                vehicleDevice.VendorDeviceId,
                input.AuthorizationStartTime,
                input.AuthorizationEndTime);

            await _vehicleControlAuthorizationRepository.UpdateAsync(authorization, autoSave: true);
        }

        return MapToDto(authorization);
    }

    public async Task<OpenAppVehicleAuthorizationDto> RenewAsync(Guid authorizationId, RenewOpenAppVehicleAuthorizationDto input)
    {
        await CheckManagePolicyAsync();
        Check.NotNull(input, nameof(input));

        var authorization = await _vehicleControlAuthorizationRepository.GetAsync(authorizationId);
        ValidateAuthorizationPeriod(input.AuthorizationStartTime, input.AuthorizationEndTime);

        var openApp = await GetOpenAppAsync(authorization.OpenAppId);
        ValidateOpenAppAvailability(openApp, input.AuthorizationStartTime, input.AuthorizationEndTime);

        var vehicle = await GetVehicleAsync(authorization.VehicleId);
        var vehicleDevice = await GetCurrentVehicleDeviceAsync(vehicle.Id);

        var conflictAuthorization = await _vehicleControlAuthorizationRepository.FindConflictAsync(
            vehicle.Id,
            input.AuthorizationStartTime,
            input.AuthorizationEndTime,
            authorization.Id);

        if (conflictAuthorization != null && conflictAuthorization.OpenAppId != authorization.OpenAppId)
        {
            throw new BusinessException("VNext:OpenPlatform:VehicleControlAuthorizationConflict")
                .WithData(nameof(authorizationId), authorizationId)
                .WithData(nameof(authorization.VehicleId), authorization.VehicleId)
                .WithData(nameof(input.AuthorizationStartTime), input.AuthorizationStartTime)
                .WithData(nameof(input.AuthorizationEndTime), input.AuthorizationEndTime);
        }

        authorization.Renew(
            vehicleDevice.Id,
            vehicle.Vin,
            vehicle.Vin,
            vehicleDevice.VendorDeviceId,
            input.AuthorizationStartTime,
            input.AuthorizationEndTime);

        await _vehicleControlAuthorizationRepository.UpdateAsync(authorization, autoSave: true);
        return MapToDto(authorization);
    }

    public async Task CancelAsync(Guid authorizationId, CancelOpenAppVehicleAuthorizationDto input)
    {
        await CheckManagePolicyAsync();
        Check.NotNull(input, nameof(input));

        var authorization = await _vehicleControlAuthorizationRepository.GetAsync(authorizationId);
        authorization.Cancel(input.CancelTime ?? Clock.Now);
        await _vehicleControlAuthorizationRepository.UpdateAsync(authorization, autoSave: true);
    }

    public async Task<OpenAppVehicleAuthorizationDto> GetAsync(Guid authorizationId)
    {
        await CheckManagePolicyAsync();
        var authorization = await _vehicleControlAuthorizationRepository.GetAsync(authorizationId);
        return MapToDto(authorization);
    }

    protected virtual Task CheckManagePolicyAsync()
    {
        return Task.CompletedTask;
    }

    private async Task<OpenApp> GetOpenAppAsync(Guid openAppId)
    {
        var openApp = await _openAppRepository.FindAsync(openAppId);
        if (openApp == null)
        {
            throw new BusinessException("VNext:OpenPlatform:OpenAppNotFound")
                .WithData(nameof(openAppId), openAppId);
        }

        return openApp;
    }

    private async Task<Vehicle> GetVehicleAsync(Guid vehicleId)
    {
        var vehicle = await _vehicleRepository.FindAsync(vehicleId);
        if (vehicle == null)
        {
            throw new BusinessException("VNext:OpenPlatform:VehicleNotFound")
                .WithData(nameof(vehicleId), vehicleId);
        }

        return vehicle;
    }

    private async Task<VehicleDevice> GetCurrentVehicleDeviceAsync(Guid vehicleId)
    {
        var vehicleDevice = await _vehicleDeviceRepository.FindCurrentByVehicleIdAsync(vehicleId);
        if (vehicleDevice == null)
        {
            throw new BusinessException("VNext:OpenPlatform:VehicleDeviceNotFound")
                .WithData(nameof(vehicleId), vehicleId);
        }

        return vehicleDevice;
    }

    private static void ValidateAuthorizationPeriod(DateTime startTime, DateTime endTime)
    {
        if (startTime > endTime)
        {
            throw new BusinessException("VNext:OpenPlatform:InvalidVehicleControlAuthorizationPeriod")
                .WithData(nameof(startTime), startTime)
                .WithData(nameof(endTime), endTime);
        }
    }

    private static void ValidateOpenAppAvailability(OpenApp openApp, DateTime startTime, DateTime endTime)
    {
        if (openApp.Status != OpenAppStatus.Enabled)
        {
            throw new BusinessException("VNext:OpenPlatform:OpenAppDisabled")
                .WithData(nameof(openApp.Id), openApp.Id);
        }

        if (openApp.BeginTime.HasValue && startTime < openApp.BeginTime.Value)
        {
            throw new BusinessException("VNext:OpenPlatform:OpenAppAuthorizationOutOfValidityPeriod")
                .WithData(nameof(openApp.Id), openApp.Id)
                .WithData(nameof(startTime), startTime)
                .WithData(nameof(openApp.BeginTime), openApp.BeginTime);
        }

        if (openApp.EndTime.HasValue && endTime > openApp.EndTime.Value)
        {
            throw new BusinessException("VNext:OpenPlatform:OpenAppAuthorizationOutOfValidityPeriod")
                .WithData(nameof(openApp.Id), openApp.Id)
                .WithData(nameof(endTime), endTime)
                .WithData(nameof(openApp.EndTime), openApp.EndTime);
        }
    }

    private static OpenAppVehicleAuthorizationDto MapToDto(VehicleControlAuthorization authorization)
    {
        return new OpenAppVehicleAuthorizationDto
        {
            Id = authorization.Id,
            OpenAppId = authorization.OpenAppId,
            VehicleId = authorization.VehicleId,
            VehicleDeviceId = authorization.VehicleDeviceId,
            VehicleVin = authorization.VehicleVin,
            DeviceVin = authorization.DeviceVin,
            VendorDeviceId = authorization.VendorDeviceId,
            AuthorizationStartTime = authorization.AuthorizationStartTime,
            AuthorizationEndTime = authorization.AuthorizationEndTime,
            CreationTime = authorization.CreationTime,
            CreatorId = authorization.CreatorId,
            LastModificationTime = authorization.LastModificationTime,
            LastModifierId = authorization.LastModifierId
        };
    }
}
