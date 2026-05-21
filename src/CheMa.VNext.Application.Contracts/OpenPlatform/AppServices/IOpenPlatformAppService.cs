using System.Threading.Tasks;
using CheMa.VNext.OpenPlatform.Dtos;
using CheMa.VNext.OpenPlatform.Inputs;
using Volo.Abp.Application.Services;

namespace CheMa.VNext.OpenPlatform.AppServices;

public interface IOpenPlatformAppService : IApplicationService
{
    Task<OpenPlatformResponseDto<OpenAppVehicleAuthorizationDto>> AuthorizeVehicleAsync(CreateOpenPlatformVehicleAuthorizationInput input);

    Task<OpenPlatformResponseDto<object?>> CancelVehicleAuthorizationAsync(GetVehicleControlAuthorizationInput input);

    Task<OpenPlatformResponseDto<OpenPlatformAuthorizedDto>> GetAuthorizedAsync(GetVehicleControlAuthorizationInput input);

    Task<OpenPlatformResponseDto<OpenPlatformVehicleInfoDto>> GetVehicleCurrentInfoAsync(GetVehicleControlAuthorizationInput input);

    Task<OpenPlatformResponseDto<OpenPlatformTripListDto>> GetVehicleTripsAsync(GetOpenPlatformVehicleTripsInput input);

    Task<OpenPlatformResponseDto<OpenPlatformTripTrackDto>> GetVehicleTraceAsync(GetOpenPlatformVehicleTraceInput input);

    Task<OpenPlatformResponseDto<OpenPlatformControlResultDto>> ControlAsync(OpenPlatformControlInput input);

    Task<OpenPlatformResponseDto<OpenPlatformAlarmListDto>> GetAlarmsAsync(GetOpenPlatformAlarmsInput input);

    Task<OpenPlatformResponseDto<OpenPlatformCapabilityResultDto>> EnableStartAsync(OpenPlatformVehicleCapabilityInput input);

    Task<OpenPlatformResponseDto<OpenPlatformCapabilityResultDto>> DisableStartAsync(OpenPlatformVehicleCapabilityInput input);
}
