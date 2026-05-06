using Volo.Abp.Modularity;

namespace CheMa.VNext;

[DependsOn(
    typeof(VNextDomainModule),
    typeof(VNextTestBaseModule)
)]
public class VNextDomainTestModule : AbpModule
{

}
