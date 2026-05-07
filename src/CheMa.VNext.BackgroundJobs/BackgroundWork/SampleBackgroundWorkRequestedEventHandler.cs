using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace CheMa.VNext.BackgroundWork;

public class SampleBackgroundWorkRequestedEventHandler :
    IDistributedEventHandler<SampleBackgroundWorkRequestedEto>,
    ITransientDependency
{
    private readonly ILogger<SampleBackgroundWorkRequestedEventHandler> _logger;

    public SampleBackgroundWorkRequestedEventHandler(ILogger<SampleBackgroundWorkRequestedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleEventAsync(SampleBackgroundWorkRequestedEto eventData)
    {
        _logger.LogInformation(
            "Sample distributed event handled. CorrelationId: {CorrelationId}, Message: {Message}",
            eventData.CorrelationId,
            eventData.Message);

        return Task.CompletedTask;
    }
}
