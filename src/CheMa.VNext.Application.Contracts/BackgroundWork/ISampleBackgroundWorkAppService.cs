using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace CheMa.VNext.BackgroundWork;

public interface ISampleBackgroundWorkAppService : IApplicationService
{
    Task EnqueueJobAsync(string message);

    Task PublishEventAsync(string message);
}
