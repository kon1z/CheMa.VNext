using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace CheMa.VNext.BackgroundWork;

public class SampleBackgroundJob : AsyncBackgroundJob<SampleJobArgs>, ITransientDependency
{
    private readonly IBackgroundExecutionContextRunner _runner;
    private readonly ILogger<SampleBackgroundJob> _logger;

    public SampleBackgroundJob(
        IBackgroundExecutionContextRunner runner,
        ILogger<SampleBackgroundJob> logger)
    {
        _runner = runner;
        _logger = logger;
    }

    public override async Task ExecuteAsync(SampleJobArgs args)
    {
        await _runner.RunAsync(args.ExecutionContext, async () =>
        {
            _logger.LogInformation(
                "Sample background job executed. CorrelationId: {CorrelationId}, Message: {Message}, TenantId: {TenantId}, OperatorUserId: {OperatorUserId}",
                args.CorrelationId,
                args.Message,
                args.ExecutionContext.TenantId,
                args.ExecutionContext.OperatorUserId);
            await Task.CompletedTask;
        });
    }
}
