using Volo.Abp.Modularity;

namespace CheMa.VNext;

[DependsOn(
    typeof(VNextApplicationModule),
    typeof(VNextDomainTestModule)
)]
public class VNextApplicationTestModule : AbpModule
{

}
