using System;
using System.Threading.Tasks;
using CheMa.VNext.OpenPlatform.Dtos;
using CheMa.VNext.OpenPlatform.Inputs;
using Volo.Abp.Application.Services;

namespace CheMa.VNext.OpenPlatform.AppServices;

public interface IOpenAppVehicleAuthorizationAppService : IApplicationService
{
    Task<OpenAppVehicleAuthorizationDto> AuthorizeAsync(Guid openAppId, AuthorizeOpenAppVehicleDto input);

    Task<OpenAppVehicleAuthorizationDto> RenewAsync(Guid authorizationId, RenewOpenAppVehicleAuthorizationDto input);

    Task CancelAsync(Guid authorizationId, CancelOpenAppVehicleAuthorizationDto input);

    Task<OpenAppVehicleAuthorizationDto> GetAsync(Guid authorizationId);
}
