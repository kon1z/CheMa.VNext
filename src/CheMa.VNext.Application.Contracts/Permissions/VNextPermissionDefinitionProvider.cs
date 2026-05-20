using CheMa.VNext.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace CheMa.VNext.Permissions;

public class VNextPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(VNextPermissions.GroupName);
        var openPlatform = group.AddPermission(OpenPlatformPermissions.GroupName, L("Permission:OpenPlatform"));
        var openApps = openPlatform.AddChild(OpenPlatformPermissions.OpenApps.Default, L("Permission:OpenPlatform.OpenApps"));

        openApps.AddChild(OpenPlatformPermissions.OpenApps.Create, L("Permission:OpenPlatform.OpenApps.Create"));
        openApps.AddChild(OpenPlatformPermissions.OpenApps.Update, L("Permission:OpenPlatform.OpenApps.Update"));
        openApps.AddChild(OpenPlatformPermissions.OpenApps.Enable, L("Permission:OpenPlatform.OpenApps.Enable"));
        openApps.AddChild(OpenPlatformPermissions.OpenApps.Disable, L("Permission:OpenPlatform.OpenApps.Disable"));
        openApps.AddChild(OpenPlatformPermissions.OpenApps.ResetSecret, L("Permission:OpenPlatform.OpenApps.ResetSecret"));
        var vehicleAuthorizations = openPlatform.AddChild(OpenPlatformPermissions.VehicleAuthorizations.Default, L("Permission:OpenPlatform.VehicleAuthorizations"));
        vehicleAuthorizations.AddChild(OpenPlatformPermissions.VehicleAuthorizations.Manage, L("Permission:OpenPlatform.VehicleAuthorizations.Manage"));
        openPlatform.AddChild(OpenPlatformPermissions.AccessLogs.Default, L("Permission:OpenPlatform.AccessLogs"));
        openPlatform.AddChild(OpenPlatformPermissions.SignatureDebug.Default, L("Permission:OpenPlatform.SignatureDebug"));

        var vehicleCapabilities = group.AddPermission(VehicleCapabilityPermissions.Default, L("Permission:VehicleCapabilities"));
        var vehicleCapabilityView = vehicleCapabilities.AddChild(VehicleCapabilityPermissions.View, L("Permission:VehicleCapabilities.View"));
        vehicleCapabilityView.AddChild(VehicleCapabilityPermissions.ViewInfo, L("Permission:VehicleCapabilities.ViewInfo"));
        vehicleCapabilityView.AddChild(VehicleCapabilityPermissions.ViewStatus, L("Permission:VehicleCapabilities.ViewStatus"));
        vehicleCapabilityView.AddChild(VehicleCapabilityPermissions.ViewLocation, L("Permission:VehicleCapabilities.ViewLocation"));
        vehicleCapabilities.AddChild(VehicleCapabilityPermissions.Control, L("Permission:VehicleCapabilities.Control"));
    }

    private static LocalizableString L(string name) => LocalizableString.Create<VNextResource>(name);
}
