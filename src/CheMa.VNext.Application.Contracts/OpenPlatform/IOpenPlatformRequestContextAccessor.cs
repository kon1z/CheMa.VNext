namespace CheMa.VNext.OpenPlatform;

public interface IOpenPlatformRequestContextAccessor
{
    OpenPlatformRequestContext? Current { get; set; }
}
