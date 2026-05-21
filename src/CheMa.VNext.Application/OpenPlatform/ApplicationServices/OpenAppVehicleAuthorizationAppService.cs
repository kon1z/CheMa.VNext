using System;
using System.Threading.Tasks;
using CheMa.VNext.Base;
using CheMa.VNext.OpenPlatform.AppServices;
using CheMa.VNext.OpenPlatform.Dtos;
using CheMa.VNext.OpenPlatform.Entities;
using CheMa.VNext.OpenPlatform.Enums;
using CheMa.VNext.OpenPlatform.Inputs;
using CheMa.VNext.OpenPlatform.Repositories;
using CheMa.VNext.VehicleDevices.Entities;
using CheMa.VNext.VehicleDevices.Repositories;
using CheMa.VNext.Vehicles;
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
    private readonly IOpenPlatformRequestContextAccessor _openPlatformRequestContextAccessor;

    public OpenAppVehicleAuthorizationAppService(
        IRepository<OpenApp, Guid> openAppRepository,
        IVehicleRepository vehicleRepository,
        IVehicleDeviceRepository vehicleDeviceRepository,
        IVehicleControlAuthorizationRepository vehicleControlAuthorizationRepository,
        IOpenPlatformRequestContextAccessor openPlatformRequestContextAccessor)
    {
        _openAppRepository = openAppRepository;
        _vehicleRepository = vehicleRepository;
        _vehicleDeviceRepository = vehicleDeviceRepository;
        _vehicleControlAuthorizationRepository = vehicleControlAuthorizationRepository;
        _openPlatformRequestContextAccessor = openPlatformRequestContextAccessor;
    }

    public async Task<OpenAppVehicleAuthorizationDto> AuthorizeAsync(Guid openAppId, AuthorizeOpenAppVehicleDto input)
    {
        await CheckManagePolicyAsync();
        Check.NotNull(input, nameof(input));

        var openApp = await GetOpenAppAsync(openAppId);
        var vehicle = await GetVehicleAsync(input.VehicleId);
        return await AuthorizeAsync(openApp, vehicle, input.AuthorizationStartTime, input.AuthorizationEndTime);
    }

    public async Task<OpenPlatformResponseDto<OpenAppVehicleAuthorizationDto>> AuthorizeCurrentOpenAppAsync(CreateOpenPlatformVehicleAuthorizationInput input)
    {
        Check.NotNull(input, nameof(input));
        Check.NotNullOrWhiteSpace(input.Vin, nameof(input.Vin), VehicleConsts.MaxVinLength);

        var openApp = await GetCurrentOpenAppAsync();
        var vehicle = await GetVehicleAsync(input.Vin);
        var authorization = await AuthorizeAsync(openApp, vehicle, input.AuthorizationStartTime ?? Clock.Now, input.AuthorizationEndTime);

        return new OpenPlatformResponseDto<OpenAppVehicleAuthorizationDto>
        {
            Data = authorization,
            TraceId = _openPlatformRequestContextAccessor.Current?.Nonce
        };
    }

    public async Task<OpenPlatformResponseDto<object?>> CancelCurrentOpenAppAsync(GetVehicleControlAuthorizationInput input)
    {
        Check.NotNull(input, nameof(input));
        Check.NotNullOrWhiteSpace(input.Vin, nameof(input.Vin), VehicleConsts.MaxVinLength);

        var openApp = await GetCurrentOpenAppAsync();
        var authorization = await _vehicleControlAuthorizationRepository.FindByOpenAppIdAndVinAsync(openApp.Id, input.Vin);
        if (authorization != null)
        {
            var now = Clock.Now;
            if (now < authorization.AuthorizationStartTime)
            {
                await _vehicleControlAuthorizationRepository.DeleteAsync(authorization, autoSave: true);
            }
            else if (!authorization.AuthorizationEndTime.HasValue || now <= authorization.AuthorizationEndTime.Value)
            {
                authorization.Cancel(now);
                await _vehicleControlAuthorizationRepository.UpdateAsync(authorization, autoSave: true);
            }
        }

        return new OpenPlatformResponseDto<object?>
        {
            TraceId = _openPlatformRequestContextAccessor.Current?.Nonce
        };
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
                .WithData(nameof(input.AuthorizationEndTime), input.AuthorizationEndTime?.ToString("O") ?? string.Empty);
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

    private async Task<OpenApp> GetCurrentOpenAppAsync()
    {
        var clientId = _openPlatformRequestContextAccessor.Current?.ClientId;
        Check.NotNullOrWhiteSpace(clientId, nameof(OpenPlatformRequestContext.ClientId), OpenPlatformConsts.MaxClientIdLength);

        var openApp = await _openAppRepository.FirstOrDefaultAsync(x => x.ClientId == clientId);
        if (openApp == null)
        {
            throw new BusinessException("VNext:OpenPlatform:ClientIdNotFound")
                .WithData(nameof(clientId), clientId);
        }

        return openApp;
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

    private async Task<Vehicle> GetVehicleAsync(string vin)
    {
        var vehicle = await _vehicleRepository.FirstOrDefaultAsync(x => x.Vin == vin);
        if (vehicle == null)
        {
            throw new BusinessException("VNext:OpenPlatform:VehicleNotFound")
                .WithData(nameof(vin), vin);
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

    private async Task<OpenAppVehicleAuthorizationDto> AuthorizeAsync(
        OpenApp openApp,
        Vehicle vehicle,
        DateTime authorizationStartTime,
        DateTime? authorizationEndTime)
    {
        ValidateAuthorizationPeriod(authorizationStartTime, authorizationEndTime);
        ValidateOpenAppAvailability(openApp, authorizationStartTime, authorizationEndTime);

        var vehicleDevice = await GetCurrentVehicleDeviceAsync(vehicle.Id);

        var conflictAuthorization = await _vehicleControlAuthorizationRepository.FindConflictAsync(
            vehicle.Id,
            authorizationStartTime,
            authorizationEndTime);

        if (conflictAuthorization != null && conflictAuthorization.OpenAppId != openApp.Id)
        {
            throw new BusinessException("VNext:OpenPlatform:VehicleControlAuthorizationConflict")
                .WithData(nameof(openApp.Id), openApp.Id)
                .WithData(nameof(vehicle.Id), vehicle.Id)
                .WithData(nameof(authorizationStartTime), authorizationStartTime)
                .WithData(nameof(authorizationEndTime), authorizationEndTime?.ToString("O") ?? string.Empty);
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
                authorizationStartTime,
                authorizationEndTime);

            await _vehicleControlAuthorizationRepository.InsertAsync(authorization, autoSave: true);
        }
        else
        {
            authorization.Renew(
                vehicleDevice.Id,
                vehicle.Vin,
                vehicle.Vin,
                vehicleDevice.VendorDeviceId,
                authorizationStartTime,
                authorizationEndTime);

            await _vehicleControlAuthorizationRepository.UpdateAsync(authorization, autoSave: true);
        }

        return MapToDto(authorization);
    }

    private static void ValidateAuthorizationPeriod(DateTime startTime, DateTime? endTime)
    {
        if (endTime.HasValue && startTime > endTime.Value)
        {
            throw new BusinessException("VNext:OpenPlatform:InvalidVehicleControlAuthorizationPeriod")
                .WithData(nameof(startTime), startTime)
                .WithData(nameof(endTime), endTime?.ToString("O") ?? string.Empty);
        }
    }

    private static void ValidateOpenAppAvailability(OpenApp openApp, DateTime startTime, DateTime? endTime)
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

        if (openApp.EndTime.HasValue && (!endTime.HasValue || endTime.Value > openApp.EndTime.Value))
        {
            throw new BusinessException("VNext:OpenPlatform:OpenAppAuthorizationOutOfValidityPeriod")
                .WithData(nameof(openApp.Id), openApp.Id)
                .WithData(nameof(endTime), endTime?.ToString("O") ?? string.Empty)
                .WithData(nameof(openApp.EndTime), openApp.EndTime.Value);
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
