using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace CheMa.VNext.Blazor.Client;

[Dependency(ReplaceServices = true)]
public class VNextBrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "VNext";
}
