using System.Threading.Tasks;
using CheMa.VNext.BackgroundWork;
using Volo.Abp;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.BackgroundWorkers.Quartz;
using Volo.Abp.Modularity;

namespace CheMa.VNext;

[DependsOn(
    typeof(VNextApplicationContractsModule),
    typeof(AbpBackgroundJobsAbstractionsModule),
    typeof(AbpBackgroundWorkersQuartzModule)
)]
public class VNextBackgroundJobsModule : AbpModule
{
    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        await context.AddBackgroundWorkerAsync<SampleQuartzBackgroundWorker>();
    }
}
