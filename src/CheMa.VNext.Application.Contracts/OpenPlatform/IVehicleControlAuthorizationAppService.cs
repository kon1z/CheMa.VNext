using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace CheMa.VNext.OpenPlatform;

public interface IVehicleControlAuthorizationAppService : IApplicationService
{
    Task<VehicleControlAuthorizationDto> GetAuthorizedAsync(GetVehicleControlAuthorizationInput input);
}
