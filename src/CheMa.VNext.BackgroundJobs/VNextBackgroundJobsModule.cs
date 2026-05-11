using System;
using System.Threading.Tasks;
using CheMa.VNext.BackgroundWork;
using Volo.Abp;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.BackgroundWorkers.Quartz;
using Volo.Abp.Modularity;
using Volo.Abp.Quartz;

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
            options.IsJobExecutionEnabled = false;
        });

        Configure<AbpQuartzOptions>(options =>
        {
            options.StartDelay = TimeSpan.FromSeconds(5);
        });
    }

    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context) => await context.AddBackgroundWorkerAsync<SampleQuartzBackgroundWorker>();
}
