using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Users;

namespace CheMa.VNext.VehicleCapabilities.Admin;

public class AdminVehicleAccessContextFactory : ITransientDependency
{
    private readonly ICurrentUser _currentUser;

    public AdminVehicleAccessContextFactory(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public Task<Shared.VehicleAccessContext> CreateAsync()
    {
        return Task.FromResult(new Shared.VehicleAccessContext
        {
            Channel = Shared.VehicleAccessChannel.Admin,
            UserId = _currentUser.Id,
            TenantId = _currentUser.TenantId,
            IsAdmin = true
        });
    }
}
