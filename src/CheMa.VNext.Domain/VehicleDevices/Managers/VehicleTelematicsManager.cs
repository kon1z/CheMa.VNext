using System;
using System.Threading;
using System.Threading.Tasks;
using CheMa.VNext.VehicleDevices.Entities;
using CheMa.VNext.VehicleDevices.Repositories;
using CheMa.VNext.Vehicles.Entities;
using CheMa.VNext.Vehicles.Enums;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace CheMa.VNext.VehicleDevices.Managers;

public class VehicleTelematicsManager : DomainService
{
    private readonly IVehicleDeviceRepository _vehicleDeviceRepository;

    public VehicleTelematicsManager(IVehicleDeviceRepository vehicleDeviceRepository)
    {
        _vehicleDeviceRepository = vehicleDeviceRepository;
    }

    public async Task<VehicleDevice> GetBoundDeviceAsync(
        Vehicle vehicle,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(vehicle, nameof(vehicle));

        if (vehicle.BindingStatus != VehicleBindingStatus.Bound)
        {
            throw new BusinessException(VehicleDeviceErrorCodes.BindingNotFound)
                .WithData("VehicleId", vehicle.Id);
        }

        var vehicleDevice = await _vehicleDeviceRepository.FindByVehicleIdAsync(vehicle.Id, cancellationToken);
        if (vehicleDevice == null)
        {
            throw new BusinessException(VehicleDeviceErrorCodes.BindingNotFound)
                .WithData("VehicleId", vehicle.Id);
        }

        return vehicleDevice;
    }

    public void EnsureCanUseQueryCapability(Vehicle vehicle, VehicleDevice vehicleDevice, string capabilityCode)
    {
        Check.NotNull(vehicle, nameof(vehicle));
        Check.NotNull(vehicleDevice, nameof(vehicleDevice));
        Check.NotNullOrWhiteSpace(capabilityCode, nameof(capabilityCode));

        if (vehicleDevice.VehicleId != vehicle.Id)
        {
            throw new BusinessException(VehicleDeviceErrorCodes.BindingNotFound)
                .WithData("VehicleId", vehicle.Id)
                .WithData("VehicleDeviceId", vehicleDevice.Id);
        }
    }
}
