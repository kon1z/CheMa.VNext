using System;
using System.Threading;
using System.Threading.Tasks;
using CheMa.VNext.Vehicles.Entities;
using Volo.Abp.Domain.Repositories;

namespace CheMa.VNext.Vehicles.Repositories;

public interface IVehicleRepository : IRepository<Vehicle, Guid>
{
    Task<bool> ExistsByVinAsync(
        string vin,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);
}
