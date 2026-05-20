using System.Threading.Tasks;
using CheMa.VNext.Permissions;
using CheMa.VNext.VehicleCapabilities.Shared;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Users;

namespace CheMa.VNext.VehicleCapabilities.Admin;

public class AdminVehicleCapabilityAuthorizer : ITransientDependency
{
    private readonly ICurrentUser _currentUser;
    private readonly IPermissionChecker _permissionChecker;

    public AdminVehicleCapabilityAuthorizer(
        ICurrentUser currentUser,
        IPermissionChecker permissionChecker)
    {
        _currentUser = currentUser;
        _permissionChecker = permissionChecker;
    }

    public async Task AuthorizeAsync(VehicleCapabilityRequirement requirement)
    {
        if (!_currentUser.IsAuthenticated)
        {
            throw new Volo.Abp.Authorization.AbpAuthorizationException();
        }

        if (requirement.OperationType == VehicleCapabilityOperationType.Command)
        {
            await CheckPermissionAsync(VehicleCapabilityPermissions.Control);
            return;
        }

        var permissionName = requirement.CapabilityCode switch
        {
            VehicleCapabilityCodes.VehicleLocationQuery => VehicleCapabilityPermissions.ViewLocation,
            VehicleCapabilityCodes.VehicleStatusQuery => VehicleCapabilityPermissions.ViewStatus,
            _ => VehicleCapabilityPermissions.ViewInfo
        };

        await CheckPermissionAsync(permissionName);
    }

    private async Task CheckPermissionAsync(string permissionName)
    {
        if (!await _permissionChecker.IsGrantedAsync(permissionName))
        {
            throw new Volo.Abp.Authorization.AbpAuthorizationException();
        }
    }
}
