namespace CheMa.VNext.Permissions;

public static class OpenPlatformPermissions
{
    public const string GroupName = VNextPermissions.GroupName + ".OpenPlatform";

    public static class OpenApps
    {
        public const string Default = GroupName + ".OpenApps";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Enable = Default + ".Enable";
        public const string Disable = Default + ".Disable";
        public const string ResetSecret = Default + ".ResetSecret";
    }

    public static class AccessLogs
    {
        public const string Default = GroupName + ".AccessLogs";
    }

    public static class SignatureDebug
    {
        public const string Default = GroupName + ".SignatureDebug";
    }
}
