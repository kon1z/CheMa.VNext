using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Volo.Abp.BackgroundWorkers.Quartz;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;

namespace CheMa.VNext.BackgroundWork;

public class SampleQuartzBackgroundWorker : QuartzBackgroundWorkerBase, ITransientDependency
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SampleQuartzBackgroundWorker(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;

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
        using var scope = _serviceScopeFactory.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IBackgroundExecutionContextRunner>();
        var currentTenant = scope.ServiceProvider.GetRequiredService<ICurrentTenant>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<SampleQuartzBackgroundWorker>>();

        var tenantIdValue = context.MergedJobDataMap.ContainsKey("TenantId")
            ? context.MergedJobDataMap.GetString("TenantId")
            : null;
        var tenantId = Guid.TryParse(tenantIdValue, out var parsedTenantId) ? parsedTenantId : (Guid?)null;

        try
        {
            await runner.RunAsync(new BackgroundExecutionContextDto
            {
                TenantId = tenantId,
                CorrelationId = context.FireInstanceId,
                Source = nameof(SampleQuartzBackgroundWorker)
            }, async () =>
            {
                logger.LogInformation(
                    "Sample Quartz background worker executed at {FireTimeUtc}. TenantId: {TenantId}.",
                    context.FireTimeUtc,
                    currentTenant.Id);
                await Task.CompletedTask;
            });
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Sample Quartz background worker failed. FireInstanceId: {FireInstanceId}, TenantId: {TenantId}.",
                context.FireInstanceId,
                tenantId);
            throw;
        }
    }
}
