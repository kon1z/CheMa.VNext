using CheMa.VNext.EntityFrameworkCore;
using CheMa.VNext.Modules;
using Volo.Abp.Autofac;
using Volo.Abp.Caching;
using Volo.Abp.Modularity;

namespace CheMa.VNext;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(VNextEntityFrameworkCoreModule),
    typeof(VNextBackgroundJobsModule)
)]
public class VNextBackgroundWorkerModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpDistributedCacheOptions>(options => { options.KeyPrefix = "VNext:"; });
    }
}
