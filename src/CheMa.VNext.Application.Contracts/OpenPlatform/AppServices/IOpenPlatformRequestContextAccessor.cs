using CheMa.VNext.OpenPlatform.Dtos;

namespace CheMa.VNext.OpenPlatform.AppServices;

public interface IOpenPlatformRequestContextAccessor
{
    OpenPlatformRequestContext? Current { get; set; }
}
