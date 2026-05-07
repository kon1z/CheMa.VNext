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
        await _backgroundJobManager.EnqueueAsync(new SampleJobArgs
        {
            CorrelationId = Guid.NewGuid(),
            Message = message
        });
    }

    public async Task PublishEventAsync(string message)
    {
        await _distributedEventBus.PublishAsync(new SampleBackgroundWorkRequestedEto
        {
            CorrelationId = Guid.NewGuid(),
            Message = message
        });
    }
}
