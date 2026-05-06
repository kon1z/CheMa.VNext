using CheMa.VNext.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace CheMa.VNext.Permissions;

public class VNextPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(VNextPermissions.GroupName);
        //Define your own permissions here. Example:
        //myGroup.AddPermission(VNextPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<VNextResource>(name);
    }
}
