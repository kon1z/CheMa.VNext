using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace CheMa.VNext.VehicleDevices;

public interface IVehicleDeviceRepository : IRepository<VehicleDevice, Guid>
{
    Task<VehicleDevice?> FindBoundByVehicleIdAsync(
        Guid vehicleId,
        CancellationToken cancellationToken = default);

    Task<VehicleDevice?> FindBoundByVendorDeviceAsync(
        string brand,
        string vendorDeviceId,
        CancellationToken cancellationToken = default);
}
