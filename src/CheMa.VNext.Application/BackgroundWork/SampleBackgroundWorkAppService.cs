using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.EventBus.Distributed;

namespace CheMa.VNext.BackgroundWork;

public class SampleBackgroundWorkAppService : ApplicationService, ISampleBackgroundWorkAppService
{
    private readonly IBackgroundJobManager _backgroundJobManager;
    private readonly IDistributedEventBus _distributedEventBus;

    public SampleBackgroundWorkAppService(
        IBackgroundJobManager backgroundJobManager,
        IDistributedEventBus distributedEventBus)
    {
        _backgroundJobManager = backgroundJobManager;
        _distributedEventBus = distributedEventBus;
    }

    public async Task EnqueueJobAsync(string message)
    {
        var correlationId = Guid.NewGuid();
        await _backgroundJobManager.EnqueueAsync(new SampleJobArgs
        {
            CorrelationId = correlationId,
            Message = message,
            ExecutionContext = CreateExecutionContext(correlationId, "AbpBackgroundJob")
        });
    }

    public async Task PublishEventAsync(string message)
    {
        var correlationId = Guid.NewGuid();
        await _distributedEventBus.PublishAsync(new SampleBackgroundWorkRequestedEto
        {
            CorrelationId = correlationId,
            Message = message,
            ExecutionContext = CreateExecutionContext(correlationId, "DistributedEvent")
        });
    }

    private BackgroundExecutionContextDto CreateExecutionContext(Guid correlationId, string source)
    {
        return new BackgroundExecutionContextDto
        {
            TenantId = CurrentTenant.Id,
            OperatorUserId = CurrentUser.Id,
            CorrelationId = correlationId.ToString("N"),
            Source = source
        };
    }
}
