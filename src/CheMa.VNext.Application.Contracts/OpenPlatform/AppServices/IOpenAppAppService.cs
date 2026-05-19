using System;
using System.Threading.Tasks;
using CheMa.VNext.OpenPlatform.Dtos;
using CheMa.VNext.OpenPlatform.Inputs;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace CheMa.VNext.OpenPlatform.AppServices;

public interface IOpenAppAppService : IApplicationService
{
    Task<OpenAppDto> GetAsync(Guid id);

    Task<PagedResultDto<OpenAppDto>> GetListAsync(GetOpenAppListInput input);

    Task<OpenAppSecretResultDto> CreateAsync(CreateOpenAppDto input);

    Task<OpenAppDto> UpdateAsync(Guid id, UpdateOpenAppDto input);

    Task EnableAsync(Guid id);

    Task DisableAsync(Guid id);

    Task<OpenAppSecretResultDto> ResetSecretAsync(Guid id);
}
