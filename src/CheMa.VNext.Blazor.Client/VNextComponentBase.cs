using CheMa.VNext.Localization;
using Volo.Abp.AspNetCore.Components;

namespace CheMa.VNext.Blazor.Client;

public abstract class VNextComponentBase : AbpComponentBase
{
    protected VNextComponentBase()
    {
        LocalizationResource = typeof(VNextResource);
    }
}
