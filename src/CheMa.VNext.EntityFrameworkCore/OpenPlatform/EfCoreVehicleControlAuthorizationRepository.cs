using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CheMa.VNext.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace CheMa.VNext.OpenPlatform;

public class EfCoreVehicleControlAuthorizationRepository
    : EfCoreRepository<VNextDbContext, VehicleControlAuthorization, Guid>, IVehicleControlAuthorizationRepository
{
    public EfCoreVehicleControlAuthorizationRepository(IDbContextProvider<VNextDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<VehicleControlAuthorization?> FindAsync(
        Guid openAppId,
        Guid vehicleDeviceId,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();

        return await dbSet
            .Where(x => x.OpenAppId == openAppId && x.VehicleDeviceId == vehicleDeviceId)
            .FirstOrDefaultAsync(GetCancellationToken(cancellationToken));
    }

    public async Task<VehicleControlAuthorization?> FindByOpenAppIdAndVinAsync(
        Guid openAppId,
        string vin,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();

        return await dbSet
            .Where(x => x.OpenAppId == openAppId && x.VehicleVin == vin)
            .OrderByDescending(x => x.AuthorizationStartTime)
            .FirstOrDefaultAsync(GetCancellationToken(cancellationToken));
    }

    public async Task<VehicleControlAuthorization?> FindConflictAsync(
        Guid vehicleId,
        DateTime startTime,
        DateTime endTime,
        Guid? excludeAuthorizationId = null,
        CancellationToken cancellationToken = default)
    {
        if (startTime > endTime)
        {
            throw new BusinessException("VNext:OpenPlatform:InvalidVehicleControlAuthorizationPeriod")
                .WithData(nameof(startTime), startTime)
                .WithData(nameof(endTime), endTime);
        }

        var dbSet = await GetDbSetAsync();

        return await dbSet
            .Where(x => x.VehicleId == vehicleId
                && (!excludeAuthorizationId.HasValue || x.Id != excludeAuthorizationId.Value)
                && x.AuthorizationStartTime <= endTime
                && (!x.AuthorizationEndTime.HasValue || startTime <= x.AuthorizationEndTime.Value))
            .OrderByDescending(x => x.AuthorizationStartTime)
            .FirstOrDefaultAsync(GetCancellationToken(cancellationToken));
    }

    public async Task<VehicleControlAuthorization?> FindByOpenAppIdAndVehicleIdAsync(
        Guid openAppId,
        Guid vehicleId,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();

        return await dbSet
            .Where(x => x.OpenAppId == openAppId && x.VehicleId == vehicleId)
            .OrderByDescending(x => x.AuthorizationStartTime)
            .FirstOrDefaultAsync(GetCancellationToken(cancellationToken));
    }

    public async Task<VehicleControlAuthorization?> FindCurrentByOpenAppIdAndVinAsync(
        Guid openAppId,
        string vin,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();

        return await dbSet
            .Where(x => x.OpenAppId == openAppId
                && x.VehicleVin == vin
                && x.AuthorizationStartTime <= now
                && (!x.AuthorizationEndTime.HasValue || now <= x.AuthorizationEndTime.Value))
            .OrderByDescending(x => x.AuthorizationStartTime)
            .FirstOrDefaultAsync(GetCancellationToken(cancellationToken));
    }
}
