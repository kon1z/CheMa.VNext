using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers.Quartz;
using Volo.Abp.Modularity;

namespace CheMa.VNext;

[DependsOn(
    typeof(VNextDomainModule),
    typeof(AbpBackgroundJobsAbstractionsModule),
    typeof(AbpBackgroundWorkersQuartzModule)
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
}
