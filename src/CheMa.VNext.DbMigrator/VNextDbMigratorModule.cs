using CheMa.VNext.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Caching;
using Volo.Abp.Modularity;

namespace CheMa.VNext.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(VNextEntityFrameworkCoreModule),
    typeof(VNextApplicationContractsModule)
    )]
public class VNextDbMigratorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpDistributedCacheOptions>(options => { options.KeyPrefix = "VNext:"; });
    }
}
