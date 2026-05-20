using System.Threading.Tasks;
using CheMa.VNext.OpenPlatform.Dtos;
using CheMa.VNext.OpenPlatform.Inputs;
using Volo.Abp.Application.Services;

namespace CheMa.VNext.OpenPlatform.AppServices;

public interface IOpenPlatformAppService : IApplicationService
{
    Task<VehicleControlAuthorizationDto> GetAuthorizedAsync(GetVehicleControlAuthorizationInput input);

    Task<OpenPlatformVehicleCurrentInfoDto> GetVehicleCurrentInfoAsync(GetVehicleControlAuthorizationInput input);

    Task<OpenPlatformVehicleTripsDto> GetVehicleTripsAsync(GetOpenPlatformVehicleTripsInput input);

    Task<OpenPlatformVehicleTraceDto> GetVehicleTraceAsync(GetOpenPlatformVehicleTraceInput input);
}
