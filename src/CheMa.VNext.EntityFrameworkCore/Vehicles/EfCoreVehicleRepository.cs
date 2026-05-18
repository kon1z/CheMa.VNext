using System;
using System.Threading;
using System.Threading.Tasks;
using CheMa.VNext.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace CheMa.VNext.Vehicles;

public class EfCoreVehicleRepository : EfCoreRepository<VNextDbContext, Vehicle, Guid>, IVehicleRepository
{
    public EfCoreVehicleRepository(IDbContextProvider<VNextDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<bool> ExistsByVinAsync(
        string vin,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();

        return await dbSet.AnyAsync(
            x => x.Vin == vin && (!excludeId.HasValue || x.Id != excludeId.Value),
            cancellationToken);
    }
}
