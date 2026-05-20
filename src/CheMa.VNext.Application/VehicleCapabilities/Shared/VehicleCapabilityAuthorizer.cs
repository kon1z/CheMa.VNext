using System.Threading.Tasks;
using CheMa.VNext.VehicleCapabilities.Admin;
using CheMa.VNext.VehicleCapabilities.OpenPlatform;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace CheMa.VNext.VehicleCapabilities.Shared;

public class VehicleCapabilityAuthorizer : IVehicleCapabilityAuthorizer, ITransientDependency
{
    private readonly AdminVehicleCapabilityAuthorizer _adminAuthorizer;
    private readonly OpenPlatformVehicleCapabilityAuthorizer _openPlatformAuthorizer;

    public VehicleCapabilityAuthorizer(
        AdminVehicleCapabilityAuthorizer adminAuthorizer,
        OpenPlatformVehicleCapabilityAuthorizer openPlatformAuthorizer)
    {
        _adminAuthorizer = adminAuthorizer;
        _openPlatformAuthorizer = openPlatformAuthorizer;
    }

    public Task AuthorizeAsync(VehicleAccessContext context, VehicleCapabilityRequirement requirement)
    {
        return context.Channel switch
        {
            VehicleAccessChannel.Admin => _adminAuthorizer.AuthorizeAsync(requirement),
            VehicleAccessChannel.OpenPlatform => _openPlatformAuthorizer.AuthorizeAsync(context, requirement),
            _ => throw new BusinessException("VNext:VehicleCapability:UnsupportedAccessChannel")
        };
    }
}
