using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace CheMa.VNext.OpenPlatform;

public interface IOpenPlatformSignatureDebugAppService : IApplicationService
{
    Task<OpenPlatformSignatureDebugResultDto> GenerateAsync(OpenPlatformSignatureDebugInput input);
}
