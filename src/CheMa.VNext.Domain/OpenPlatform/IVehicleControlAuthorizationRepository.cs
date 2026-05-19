using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace CheMa.VNext.OpenPlatform;

public interface IVehicleControlAuthorizationRepository : IRepository<VehicleControlAuthorization, Guid>
{
    Task<VehicleControlAuthorization?> FindAsync(
        Guid openAppId,
        Guid vehicleDeviceId,
        CancellationToken cancellationToken = default);

    Task<VehicleControlAuthorization?> FindByOpenAppIdAndVinAsync(
        Guid openAppId,
        string vin,
        CancellationToken cancellationToken = default);

    Task<VehicleControlAuthorization?> FindConflictAsync(
        Guid vehicleId,
        DateTime startTime,
        DateTime endTime,
        Guid? excludeAuthorizationId = null,
        CancellationToken cancellationToken = default);

    Task<VehicleControlAuthorization?> FindByOpenAppIdAndVehicleIdAsync(
        Guid openAppId,
        Guid vehicleId,
        CancellationToken cancellationToken = default);

    Task<VehicleControlAuthorization?> FindCurrentByOpenAppIdAndVinAsync(
        Guid openAppId,
        string vin,
        DateTime now,
        CancellationToken cancellationToken = default);
}
