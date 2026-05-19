using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CheMa.VNext.EntityFrameworkCore;
using CheMa.VNext.Vehicles;
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

    public async Task<VehicleDevice?> FindByVehicleIdAsync(
        Guid vehicleId,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();

        return await dbSet
            .Where(x => x.VehicleId == vehicleId)
            .FirstOrDefaultAsync(GetCancellationToken(cancellationToken));
    }

    public async Task<VehicleDevice?> FindByVendorDeviceAsync(
        VehicleDeviceVendorType vendorType,
        string vendorDeviceId,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();

        return await dbSet
            .Where(x => x.VendorType == vendorType
                && x.VendorDeviceId == vendorDeviceId)
            .FirstOrDefaultAsync(GetCancellationToken(cancellationToken));
    }
}
