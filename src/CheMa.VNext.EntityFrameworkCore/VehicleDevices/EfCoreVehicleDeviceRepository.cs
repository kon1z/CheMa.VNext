using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CheMa.VNext.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace CheMa.VNext.VehicleDevices;

public class EfCoreVehicleDeviceRepository
    : EfCoreRepository<VNextDbContext, VehicleDevice, Guid>, IVehicleDeviceRepository
{
    public EfCoreVehicleDeviceRepository(IDbContextProvider<VNextDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<VehicleDevice?> FindBoundByVehicleIdAsync(
        Guid vehicleId,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();

        return await dbSet
            .Where(x => x.VehicleId == vehicleId && x.Status == VehicleDeviceStatus.Bound)
            .FirstOrDefaultAsync(GetCancellationToken(cancellationToken));
    }

    public async Task<VehicleDevice?> FindBoundByVendorDeviceAsync(
        string brand,
        string vendorDeviceId,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();

        return await dbSet
            .Where(x => x.Brand == brand
                && x.VendorDeviceId == vendorDeviceId
                && x.Status == VehicleDeviceStatus.Bound)
            .FirstOrDefaultAsync(GetCancellationToken(cancellationToken));
    }
}
