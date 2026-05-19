using System.Threading.Tasks;
using CheMa.VNext.OpenPlatform.Dtos;
using CheMa.VNext.OpenPlatform.Inputs;
using Volo.Abp.Application.Services;

namespace CheMa.VNext.OpenPlatform.AppServices;

public interface IOpenPlatformSignatureDebugAppService : IApplicationService
{
    Task<OpenPlatformSignatureDebugResultDto> GenerateAsync(OpenPlatformSignatureDebugInput input);
}
