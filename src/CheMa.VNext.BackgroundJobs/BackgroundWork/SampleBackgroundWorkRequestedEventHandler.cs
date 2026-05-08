using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace CheMa.VNext.BackgroundWork;

public class SampleBackgroundWorkRequestedEventHandler :
    IDistributedEventHandler<SampleBackgroundWorkRequestedEto>,
    ITransientDependency
{
    private readonly IBackgroundExecutionContextRunner _runner;
    private readonly ILogger<SampleBackgroundWorkRequestedEventHandler> _logger;

    public SampleBackgroundWorkRequestedEventHandler(
        IBackgroundExecutionContextRunner runner,
        ILogger<SampleBackgroundWorkRequestedEventHandler> logger)
    {
        _runner = runner;
        _logger = logger;
    }

    public async Task HandleEventAsync(SampleBackgroundWorkRequestedEto eventData)
    {
        await _runner.RunAsync(eventData.ExecutionContext, async () =>
        {
            _logger.LogInformation(
                "Sample distributed event handled. CorrelationId: {CorrelationId}, Message: {Message}, TenantId: {TenantId}, OperatorUserId: {OperatorUserId}",
                eventData.CorrelationId,
                eventData.Message,
                eventData.ExecutionContext.TenantId,
                eventData.ExecutionContext.OperatorUserId);
            await Task.CompletedTask;
        });
    }
}
