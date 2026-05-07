using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace CheMa.VNext.BackgroundWork;

public class SampleBackgroundJob : AsyncBackgroundJob<SampleJobArgs>, ITransientDependency
{
    private readonly ILogger<SampleBackgroundJob> _logger;

    public SampleBackgroundJob(ILogger<SampleBackgroundJob> logger)
    {
        _logger = logger;
    }

    public override Task ExecuteAsync(SampleJobArgs args)
    {
        _logger.LogInformation(
            "Sample background job executed. CorrelationId: {CorrelationId}, Message: {Message}",
            args.CorrelationId,
            args.Message);

        return Task.CompletedTask;
    }
}
