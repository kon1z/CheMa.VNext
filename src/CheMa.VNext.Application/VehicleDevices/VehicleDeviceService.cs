using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CheMa.VNext.VehicleDevices.Models;
using CheMa.VNext.VehicleDevices.Providers;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace CheMa.VNext.VehicleDevices;

public class VehicleDeviceService : IVehicleDeviceService, ITransientDependency
{
    private readonly IVehicleDeviceRepository _vehicleDeviceRepository;
    private readonly IVehicleDeviceProviderResolver _providerResolver;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ILogger<VehicleDeviceService> _logger;

    public VehicleDeviceService(
        IVehicleDeviceRepository vehicleDeviceRepository,
        IVehicleDeviceProviderResolver providerResolver,
        IGuidGenerator guidGenerator,
        ILogger<VehicleDeviceService> logger)
    {
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
        Check.NotNullOrWhiteSpace(command.Brand, nameof(command.Brand), VehicleDeviceConsts.MaxBrandLength);
        Check.NotNullOrWhiteSpace(command.VendorDeviceId, nameof(command.VendorDeviceId), VehicleDeviceConsts.MaxVendorDeviceIdLength);
        Check.NotNullOrWhiteSpace(command.Vin, nameof(command.Vin), VehicleDeviceConsts.MaxVinLength);

        var currentVehicleDevice = await _vehicleDeviceRepository.FindBoundByVehicleIdAsync(
            command.VehicleId,
            cancellationToken);

        if (currentVehicleDevice != null)
        {
            if (string.Equals(currentVehicleDevice.Brand, command.Brand, StringComparison.OrdinalIgnoreCase)
                && string.Equals(currentVehicleDevice.VendorDeviceId, command.VendorDeviceId, StringComparison.OrdinalIgnoreCase))
            {
                return new BindVehicleDeviceResult
                {
                    VehicleId = command.VehicleId,
                    Brand = currentVehicleDevice.Brand,
                    VendorDeviceId = currentVehicleDevice.VendorDeviceId,
                    Success = true,
                    AlreadyBound = true
                };
            }

            throw new BusinessException(VehicleDeviceErrorCodes.VehicleAlreadyBound)
                .WithData("VehicleId", command.VehicleId)
                .WithData("Brand", currentVehicleDevice.Brand)
                .WithData("VendorDeviceId", currentVehicleDevice.VendorDeviceId);
        }

        var currentVendorDevice = await _vehicleDeviceRepository.FindBoundByVendorDeviceAsync(
            command.Brand,
            command.VendorDeviceId,
            cancellationToken);

        if (currentVendorDevice != null)
        {
            throw new BusinessException(VehicleDeviceErrorCodes.DeviceAlreadyBound)
                .WithData("VehicleId", currentVendorDevice.VehicleId)
                .WithData("Brand", command.Brand)
                .WithData("VendorDeviceId", command.VendorDeviceId);
        }

        var provider = _providerResolver.Resolve(command.Brand);
        var bindingContext = new VehicleDeviceBindingContext
        {
            VehicleId = command.VehicleId,
            Brand = command.Brand,
            VendorDeviceId = command.VendorDeviceId,
            Vin = command.Vin
        };

        await provider.BindAsync(bindingContext, cancellationToken);

        var vehicleDevice = new VehicleDevice(
            _guidGenerator.Create(),
            command.VehicleId,
            command.Brand,
            command.VendorDeviceId,
            command.Vin);

        await _vehicleDeviceRepository.InsertAsync(vehicleDevice, autoSave: true, cancellationToken);

        return new BindVehicleDeviceResult
        {
            VehicleId = command.VehicleId,
            Brand = command.Brand,
            VendorDeviceId = command.VendorDeviceId,
            Success = true
        };
    }

    public async Task<UnbindVehicleDeviceResult> UnbindAsync(
        UnbindVehicleDeviceCommand command,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(command, nameof(command));

        var vehicleDevice = await GetBoundVehicleDeviceAsync(command.VehicleId, cancellationToken);
        var provider = _providerResolver.Resolve(vehicleDevice.Brand);

        await provider.UnbindAsync(CreateBindingContext(vehicleDevice), cancellationToken);

        vehicleDevice.Unbind();
        await _vehicleDeviceRepository.UpdateAsync(vehicleDevice, autoSave: true, cancellationToken);

        return new UnbindVehicleDeviceResult
        {
            VehicleId = vehicleDevice.VehicleId,
            Brand = vehicleDevice.Brand,
            VendorDeviceId = vehicleDevice.VendorDeviceId,
            Success = true
        };
    }

    public async Task<VehicleDeviceLocationResult> GetLocationAsync(
        Guid vehicleId,
        CancellationToken cancellationToken = default)
    {
        var vehicleDevice = await GetBoundVehicleDeviceAsync(vehicleId, cancellationToken);
        var provider = _providerResolver.Resolve(vehicleDevice.Brand);

        return await provider.GetLocationAsync(CreateContext(vehicleDevice), cancellationToken);
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

        var vehicleDevice = await GetBoundVehicleDeviceAsync(query.VehicleId, cancellationToken);
        var provider = _providerResolver.Resolve(vehicleDevice.Brand);

        return await provider.GetTrackAsync(CreateContext(vehicleDevice), query, cancellationToken);
    }

    public async Task<VehicleDeviceStatusResult> GetStatusAsync(
        Guid vehicleId,
        CancellationToken cancellationToken = default)
    {
        var vehicleDevice = await GetBoundVehicleDeviceAsync(vehicleId, cancellationToken);
        var provider = _providerResolver.Resolve(vehicleDevice.Brand);

        return await provider.GetStatusAsync(CreateContext(vehicleDevice), cancellationToken);
    }

    public async Task<VehicleDeviceControlResult> ControlAsync(
        VehicleDeviceControlCommand command,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(command, nameof(command));

        var vehicleDevice = await GetBoundVehicleDeviceAsync(command.VehicleId, cancellationToken);
        var provider = _providerResolver.Resolve(vehicleDevice.Brand);

        if (!provider.SupportsControlAction(command.Action))
        {
            throw new BusinessException(VehicleDeviceErrorCodes.CapabilityNotSupported)
                .WithData("Brand", vehicleDevice.Brand)
                .WithData("Action", command.Action);
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await provider.ControlAsync(CreateContext(vehicleDevice), command.Action, cancellationToken);
            stopwatch.Stop();

            _logger.LogInformation(
                "Vehicle device control command executed. VehicleId: {VehicleId}, Brand: {Brand}, VendorDeviceId: {VendorDeviceId}, Action: {Action}, Success: {Success}, ElapsedMs: {ElapsedMs}",
                vehicleDevice.VehicleId,
                vehicleDevice.Brand,
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
                "Vehicle device control command failed. VehicleId: {VehicleId}, Brand: {Brand}, VendorDeviceId: {VendorDeviceId}, Action: {Action}, ElapsedMs: {ElapsedMs}",
                vehicleDevice.VehicleId,
                vehicleDevice.Brand,
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
        var vehicleDevice = await _vehicleDeviceRepository.FindBoundByVehicleIdAsync(vehicleId, cancellationToken);
        if (vehicleDevice == null)
        {
            throw new BusinessException(VehicleDeviceErrorCodes.BindingNotFound)
                .WithData("VehicleId", vehicleId);
        }

        return vehicleDevice;
    }

    private static VehicleDeviceBindingContext CreateBindingContext(VehicleDevice vehicleDevice)
    {
        return new VehicleDeviceBindingContext
        {
            VehicleId = vehicleDevice.VehicleId,
            Brand = vehicleDevice.Brand,
            VendorDeviceId = vehicleDevice.VendorDeviceId,
            Vin = vehicleDevice.Vin
        };
    }

    private static VehicleDeviceContext CreateContext(VehicleDevice vehicleDevice)
    {
        return new VehicleDeviceContext
        {
            VehicleId = vehicleDevice.VehicleId,
            Brand = vehicleDevice.Brand,
            VendorDeviceId = vehicleDevice.VendorDeviceId,
            Vin = vehicleDevice.Vin
        };
    }
}
