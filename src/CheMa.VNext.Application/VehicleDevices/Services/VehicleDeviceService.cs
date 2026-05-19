using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CheMa.VNext.VehicleDevices.Entities;
using CheMa.VNext.VehicleDevices.Interfaces;
using CheMa.VNext.VehicleDevices.Models;
using CheMa.VNext.VehicleDevices.Providers;
using CheMa.VNext.VehicleDevices.Repositories;
using CheMa.VNext.Vehicles;
using CheMa.VNext.Vehicles.Entities;
using CheMa.VNext.Vehicles.Repositories;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;

namespace CheMa.VNext.VehicleDevices.Services;

public class VehicleDeviceService : IVehicleDeviceService, ITransientDependency
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IVehicleDeviceRepository _vehicleDeviceRepository;
    private readonly IVehicleDeviceProviderResolver _providerResolver;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ILogger<VehicleDeviceService> _logger;

    public VehicleDeviceService(
        IVehicleRepository vehicleRepository,
        IVehicleDeviceRepository vehicleDeviceRepository,
        IVehicleDeviceProviderResolver providerResolver,
        IGuidGenerator guidGenerator,
        ILogger<VehicleDeviceService> logger)
    {
        _vehicleRepository = vehicleRepository;
        _vehicleDeviceRepository = vehicleDeviceRepository;
        _providerResolver = providerResolver;
        _guidGenerator = guidGenerator;
        _logger = logger;
    }

    public async Task<BindVehicleDeviceResult> BindAsync(
        BindVehicleDeviceCommand command,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(command, nameof(command));
        Check.NotNullOrWhiteSpace(command.VendorDeviceId, nameof(command.VendorDeviceId), VehicleDeviceConsts.MaxVendorDeviceIdLength);

        var vehicle = await _vehicleRepository.GetAsync(command.VehicleId, cancellationToken: cancellationToken);

        var currentVehicleDevice = await _vehicleDeviceRepository.FindByVehicleIdAsync(
            command.VehicleId,
            cancellationToken);

        if (vehicle.BindingStatus == VehicleBindingStatus.Bound || currentVehicleDevice != null)
        {
            if (currentVehicleDevice != null
                && currentVehicleDevice.VendorType == command.VendorType
                && string.Equals(currentVehicleDevice.VendorDeviceId, command.VendorDeviceId, StringComparison.OrdinalIgnoreCase))
            {
                return new BindVehicleDeviceResult
                {
                    VehicleId = command.VehicleId,
                    VendorType = currentVehicleDevice.VendorType,
                    VendorDeviceId = currentVehicleDevice.VendorDeviceId,
                    Success = true,
                    AlreadyBound = true
                };
            }

            throw new BusinessException(VehicleDeviceErrorCodes.VehicleAlreadyBound)
                .WithData("VehicleId", command.VehicleId)
                .WithData("VendorType", currentVehicleDevice?.VendorType.ToString() ?? string.Empty)
                .WithData("VendorDeviceId", currentVehicleDevice?.VendorDeviceId ?? string.Empty);
        }

        var currentVendorDevice = await _vehicleDeviceRepository.FindByVendorDeviceAsync(
            command.VendorType,
            command.VendorDeviceId,
            cancellationToken);

        if (currentVendorDevice?.VehicleId != null)
        {
            throw new BusinessException(VehicleDeviceErrorCodes.DeviceAlreadyBound)
                .WithData("VehicleId", currentVendorDevice.VehicleId)
                .WithData("VendorType", command.VendorType)
                .WithData("VendorDeviceId", command.VendorDeviceId);
        }

        var provider = _providerResolver.Resolve(command.VendorType);
        var bindingContext = new VehicleDeviceBindingContext
        {
            VehicleId = command.VehicleId,
            VendorType = command.VendorType,
            VendorDeviceId = command.VendorDeviceId,
            Vin = vehicle.Vin
        };

        await provider.BindAsync(bindingContext, cancellationToken);

        var vehicleDevice = currentVendorDevice ?? new VehicleDevice(
            _guidGenerator.Create(),
            command.VendorType,
            command.VendorDeviceId);

        vehicleDevice.Bind(command.VehicleId);
        vehicle.SetBindingInfo(command.VendorType, VehicleBindingStatus.Bound, DateTime.UtcNow);

        if (currentVendorDevice == null)
        {
            await _vehicleDeviceRepository.InsertAsync(vehicleDevice, autoSave: true, cancellationToken);
        }
        else
        {
            await _vehicleDeviceRepository.UpdateAsync(vehicleDevice, autoSave: true, cancellationToken: cancellationToken);
        }

        await _vehicleRepository.UpdateAsync(vehicle, autoSave: true, cancellationToken: cancellationToken);

        return new BindVehicleDeviceResult
        {
            VehicleId = command.VehicleId,
            VendorType = command.VendorType,
            VendorDeviceId = command.VendorDeviceId,
            Success = true
        };
    }

    public async Task<UnbindVehicleDeviceResult> UnbindAsync(
        UnbindVehicleDeviceCommand command,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(command, nameof(command));

        var vehicle = await _vehicleRepository.GetAsync(command.VehicleId, cancellationToken: cancellationToken);
        var vehicleDevice = await GetBoundVehicleDeviceAsync(vehicle.Id, cancellationToken);
        var provider = _providerResolver.Resolve(vehicleDevice.VendorType);

        await provider.UnbindAsync(CreateBindingContext(vehicleDevice, vehicle), cancellationToken);

        vehicle.SetBindingInfo(null, VehicleBindingStatus.Unbound, null);
        vehicleDevice.Unbind();

        await _vehicleDeviceRepository.UpdateAsync(vehicleDevice, autoSave: true, cancellationToken: cancellationToken);
        await _vehicleRepository.UpdateAsync(vehicle, autoSave: true, cancellationToken: cancellationToken);

        return new UnbindVehicleDeviceResult
        {
            VehicleId = vehicle.Id,
            VendorType = vehicleDevice.VendorType,
            VendorDeviceId = vehicleDevice.VendorDeviceId,
            Success = true
        };
    }

    public async Task<VehicleDeviceLocationResult> GetLocationAsync(
        Guid vehicleId,
        CancellationToken cancellationToken = default)
    {
        var vehicle = await _vehicleRepository.GetAsync(vehicleId, cancellationToken: cancellationToken);
        var vehicleDevice = await GetBoundVehicleDeviceAsync(vehicleId, cancellationToken);
        var provider = _providerResolver.Resolve(vehicleDevice.VendorType);

        return await provider.GetLocationAsync(CreateContext(vehicleDevice, vehicle), cancellationToken);
    }

    public async Task<VehicleDeviceTrackResult> GetTrackAsync(
        VehicleDeviceTrackQuery query,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(query, nameof(query));

        if (query.StartTimeUtc >= query.EndTimeUtc
            || query.EndTimeUtc - query.StartTimeUtc > TimeSpan.FromHours(VehicleDeviceConsts.MaxTrackQueryHours))
        {
            throw new BusinessException(VehicleDeviceErrorCodes.InvalidTrackTimeRange)
                .WithData("StartTimeUtc", query.StartTimeUtc)
                .WithData("EndTimeUtc", query.EndTimeUtc)
                .WithData("MaxHours", VehicleDeviceConsts.MaxTrackQueryHours);
        }

        var vehicle = await _vehicleRepository.GetAsync(query.VehicleId, cancellationToken: cancellationToken);
        var vehicleDevice = await GetBoundVehicleDeviceAsync(query.VehicleId, cancellationToken);
        var provider = _providerResolver.Resolve(vehicleDevice.VendorType);

        return await provider.GetTrackAsync(CreateContext(vehicleDevice, vehicle), query, cancellationToken);
    }

    public async Task<VehicleDeviceStatusResult> GetStatusAsync(
        Guid vehicleId,
        CancellationToken cancellationToken = default)
    {
        var vehicle = await _vehicleRepository.GetAsync(vehicleId, cancellationToken: cancellationToken);
        var vehicleDevice = await GetBoundVehicleDeviceAsync(vehicleId, cancellationToken);
        var provider = _providerResolver.Resolve(vehicleDevice.VendorType);

        return await provider.GetStatusAsync(CreateContext(vehicleDevice, vehicle), cancellationToken);
    }

    public async Task<VehicleDeviceControlResult> ControlAsync(
        VehicleDeviceControlCommand command,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(command, nameof(command));

        var vehicle = await _vehicleRepository.GetAsync(command.VehicleId, cancellationToken: cancellationToken);
        var vehicleDevice = await GetBoundVehicleDeviceAsync(command.VehicleId, cancellationToken);
        var provider = _providerResolver.Resolve(vehicleDevice.VendorType);

        if (!provider.SupportsControlAction(command.Action))
        {
            throw new BusinessException(VehicleDeviceErrorCodes.CapabilityNotSupported)
                .WithData("VendorType", vehicleDevice.VendorType)
                .WithData("Action", command.Action);
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await provider.ControlAsync(CreateContext(vehicleDevice, vehicle), command.Action, cancellationToken);
            stopwatch.Stop();

            _logger.LogInformation(
                "Vehicle device control command executed. VehicleId: {VehicleId}, VendorType: {VendorType}, VendorDeviceId: {VendorDeviceId}, Action: {Action}, Success: {Success}, ElapsedMs: {ElapsedMs}",
                vehicleDevice.VehicleId,
                vehicleDevice.VendorType,
                vehicleDevice.VendorDeviceId,
                command.Action,
                result.Success,
                stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Vehicle device control command failed. VehicleId: {VehicleId}, VendorType: {VendorType}, VendorDeviceId: {VendorDeviceId}, Action: {Action}, ElapsedMs: {ElapsedMs}",
                vehicleDevice.VehicleId,
                vehicleDevice.VendorType,
                vehicleDevice.VendorDeviceId,
                command.Action,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    private async Task<VehicleDevice> GetBoundVehicleDeviceAsync(
        Guid vehicleId,
        CancellationToken cancellationToken)
    {
        var vehicleDevice = await _vehicleDeviceRepository.FindByVehicleIdAsync(vehicleId, cancellationToken);
        if (vehicleDevice == null)
        {
            throw new BusinessException(VehicleDeviceErrorCodes.BindingNotFound)
                .WithData("VehicleId", vehicleId);
        }

        return vehicleDevice;
    }

    private static VehicleDeviceBindingContext CreateBindingContext(VehicleDevice vehicleDevice, Vehicle vehicle)
    {
        return new VehicleDeviceBindingContext
        {
            VehicleId = vehicle.Id,
            VendorType = vehicleDevice.VendorType,
            VendorDeviceId = vehicleDevice.VendorDeviceId,
            Vin = vehicle.Vin
        };
    }

    private static VehicleDeviceContext CreateContext(VehicleDevice vehicleDevice, Vehicle vehicle)
    {
        return new VehicleDeviceContext
        {
            VehicleId = vehicle.Id,
            VendorType = vehicleDevice.VendorType,
            VendorDeviceId = vehicleDevice.VendorDeviceId,
            Vin = vehicle.Vin
        };
    }

}
