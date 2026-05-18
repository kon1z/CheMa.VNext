using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace CheMa.VNext.Vehicles;

public interface IVehicleRepository : IRepository<Vehicle, Guid>
{
    Task<bool> ExistsByVinAsync(
        string vin,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);
}
