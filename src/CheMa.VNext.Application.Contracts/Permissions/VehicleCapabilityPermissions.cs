namespace CheMa.VNext.Permissions;

public static class VehicleCapabilityPermissions
{
    public const string GroupName = VNextPermissions.GroupName + ".VehicleCapabilities";

    public const string Default = GroupName;

    public const string View = Default + ".View";

    public const string ViewInfo = View + ".Info";

    public const string ViewStatus = View + ".Status";

    public const string ViewLocation = View + ".Location";

    public const string Control = Default + ".Control";
}
