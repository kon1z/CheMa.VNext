using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace CheMa.VNext.OpenPlatform;

public interface IOpenAppVehicleAuthorizationAppService : IApplicationService
{
    Task<OpenAppVehicleAuthorizationDto> AuthorizeAsync(Guid openAppId, AuthorizeOpenAppVehicleDto input);

    Task<OpenAppVehicleAuthorizationDto> RenewAsync(Guid authorizationId, RenewOpenAppVehicleAuthorizationDto input);

    Task CancelAsync(Guid authorizationId, CancelOpenAppVehicleAuthorizationDto input);

    Task<OpenAppVehicleAuthorizationDto> GetAsync(Guid authorizationId);
}
