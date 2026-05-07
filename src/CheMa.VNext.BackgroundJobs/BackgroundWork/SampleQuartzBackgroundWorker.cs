using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Volo.Abp.BackgroundWorkers.Quartz;
using Volo.Abp.DependencyInjection;

namespace CheMa.VNext.BackgroundWork;

public class SampleQuartzBackgroundWorker : QuartzBackgroundWorkerBase, ITransientDependency
{
    private readonly ILogger<SampleQuartzBackgroundWorker> _logger;

    public SampleQuartzBackgroundWorker(ILogger<SampleQuartzBackgroundWorker> logger)
    {
        _logger = logger;

        JobDetail = JobBuilder.Create<SampleQuartzBackgroundWorker>()
            .WithIdentity(nameof(SampleQuartzBackgroundWorker))
            .Build();

        Trigger = TriggerBuilder.Create()
            .WithIdentity($"{nameof(SampleQuartzBackgroundWorker)}Trigger")
            .WithCronSchedule("0 0/5 * * * ?")
            .Build();
    }

    public override Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Sample Quartz background worker executed at {FireTimeUtc}.", context.FireTimeUtc);
        return Task.CompletedTask;
    }
}
