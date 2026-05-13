using CheMa.VNext.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;

namespace CheMa.VNext;

[DependsOn(
    typeof(VNextDomainModule),
    typeof(AbpBackgroundJobsModule)
)]
public class VNextBackgroundJobsModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpBackgroundJobOptions>(options =>
        {
            options.IsJobExecutionEnabled = true;
        });
    }

    public override void PostConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.RemoveAll<NullTenant>();
        context.Services.RemoveAll<ICurrentTenant>();
        context.Services.RemoveAll<ICurrentTenantAccessor>();

        context.Services.AddSingleton<NullTenant>();
        context.Services.AddSingleton<ICurrentTenant>(provider => provider.GetRequiredService<NullTenant>());
        context.Services.AddSingleton<ICurrentTenantAccessor>(provider => provider.GetRequiredService<NullTenant>());
    }
}
