using System;
using System.Threading;
using System.Threading.Tasks;
using CheMa.VNext.Vehicles;
using Volo.Abp.Domain.Repositories;

namespace CheMa.VNext.VehicleDevices;

public interface IVehicleDeviceRepository : IRepository<VehicleDevice, Guid>
{
    Task<VehicleDevice?> FindByVehicleIdAsync(
        Guid vehicleId,
        CancellationToken cancellationToken = default);

    Task<VehicleDevice?> FindCurrentByVehicleIdAsync(
        Guid vehicleId,
        CancellationToken cancellationToken = default);

    Task<VehicleDevice?> FindByVendorDeviceAsync(
        VehicleDeviceVendorType vendorType,
        string vendorDeviceId,
        CancellationToken cancellationToken = default);
}
