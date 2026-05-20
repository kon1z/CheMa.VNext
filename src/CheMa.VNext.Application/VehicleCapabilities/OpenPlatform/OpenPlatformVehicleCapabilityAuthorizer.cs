using System.Threading.Tasks;
using CheMa.VNext.OpenPlatform.Repositories;
using CheMa.VNext.VehicleCapabilities.Shared;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Timing;

namespace CheMa.VNext.VehicleCapabilities.OpenPlatform;

public class OpenPlatformVehicleCapabilityAuthorizer : ITransientDependency
{
    private readonly IVehicleControlAuthorizationRepository _vehicleControlAuthorizationRepository;
    private readonly IClock _clock;

    public OpenPlatformVehicleCapabilityAuthorizer(
        IVehicleControlAuthorizationRepository vehicleControlAuthorizationRepository,
        IClock clock)
    {
        _vehicleControlAuthorizationRepository = vehicleControlAuthorizationRepository;
        _clock = clock;
    }

    public async Task AuthorizeAsync(VehicleAccessContext context, VehicleCapabilityRequirement requirement)
    {
        if (!context.OpenAppId.HasValue)
        {
            throw new BusinessException("VNext:OpenPlatform:ClientIdNotFound");
        }

        var authorization = await _vehicleControlAuthorizationRepository.FindByOpenAppIdAndVehicleIdAsync(
            context.OpenAppId.Value,
            requirement.VehicleId);
        if (authorization == null
            || authorization.AuthorizationStartTime > _clock.Now
            || authorization.AuthorizationEndTime.HasValue && authorization.AuthorizationEndTime.Value < _clock.Now)
        {
            throw new BusinessException("VNext:OpenPlatform:VehicleControlAuthorizationNotFound")
                .WithData(nameof(context.OpenAppId), context.OpenAppId)
                .WithData(nameof(requirement.VehicleId), requirement.VehicleId);
        }
    }
}
