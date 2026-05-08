using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Volo.Abp.BackgroundWorkers.Quartz;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;

namespace CheMa.VNext.BackgroundWork;

public class SampleQuartzBackgroundWorker : QuartzBackgroundWorkerBase, ITransientDependency
{
    private readonly IBackgroundExecutionContextRunner _runner;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<SampleQuartzBackgroundWorker> _logger;

    public SampleQuartzBackgroundWorker(
        IBackgroundExecutionContextRunner runner,
        ICurrentTenant currentTenant,
        ILogger<SampleQuartzBackgroundWorker> logger)
    {
        _runner = runner;
        _currentTenant = currentTenant;
        _logger = logger;

        JobDetail = JobBuilder.Create<SampleQuartzBackgroundWorker>()
            .WithIdentity(nameof(SampleQuartzBackgroundWorker))
            .Build();

        Trigger = TriggerBuilder.Create()
            .WithIdentity($"{nameof(SampleQuartzBackgroundWorker)}Trigger")
            .WithCronSchedule("0 0/5 * * * ?")
            .Build();
    }

    public override async Task Execute(IJobExecutionContext context)
    {
        var tenantIdValue = context.MergedJobDataMap.ContainsKey("TenantId")
            ? context.MergedJobDataMap.GetString("TenantId")
            : null;
        var tenantId = Guid.TryParse(tenantIdValue, out var parsedTenantId) ? parsedTenantId : (Guid?)null;

        await _runner.RunAsync(new BackgroundExecutionContextDto
        {
            TenantId = tenantId,
            CorrelationId = context.FireInstanceId,
            Source = nameof(SampleQuartzBackgroundWorker)
        }, async () =>
        {
            _logger.LogInformation(
                "Sample Quartz background worker executed at {FireTimeUtc}. TenantId: {TenantId}.",
                context.FireTimeUtc,
                _currentTenant.Id);
            await Task.CompletedTask;
        });
    }
}
