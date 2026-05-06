using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace CheMa.VNext;

[Dependency(ReplaceServices = true)]
public class VNextBrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "VNext";
}
