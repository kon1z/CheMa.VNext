using System.Threading.Tasks;
using CheMa.VNext.OpenPlatform.Dtos;
using CheMa.VNext.OpenPlatform.Inputs;
using Volo.Abp.Application.Services;

namespace CheMa.VNext.OpenPlatform.AppServices;

public interface IVehicleControlAuthorizationAppService : IApplicationService
{
    Task<VehicleControlAuthorizationDto> GetAuthorizedAsync(GetVehicleControlAuthorizationInput input);

    Task<OpenPlatformVehicleCurrentInfoDto> GetVehicleCurrentInfoAsync(GetVehicleControlAuthorizationInput input);
}
