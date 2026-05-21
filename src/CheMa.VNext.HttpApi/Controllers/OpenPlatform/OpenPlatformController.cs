using System.Threading.Tasks;
using CheMa.VNext.OpenPlatform.AppServices;
using CheMa.VNext.OpenPlatform.Dtos;
using CheMa.VNext.OpenPlatform.Inputs;
using Microsoft.AspNetCore.Mvc;

namespace CheMa.VNext.Controllers.OpenPlatform;

[Route("api/open")]
public class OpenPlatformController : VNextController
{
    private readonly IOpenPlatformAppService _openPlatformAppService;

    public OpenPlatformController(IOpenPlatformAppService openPlatformAppService)
    {
        _openPlatformAppService = openPlatformAppService;
    }

    [HttpPost("vehicle-authorizations")]
    public Task<OpenPlatformResponseDto<OpenAppVehicleAuthorizationDto>> AuthorizeVehicleAsync([FromBody] CreateOpenPlatformVehicleAuthorizationInput input)
    {
        return _openPlatformAppService.AuthorizeVehicleAsync(input);
    }

    [HttpDelete("vehicle-authorizations")]
    public Task<OpenPlatformResponseDto<object?>> CancelVehicleAuthorizationAsync([FromQuery] GetVehicleControlAuthorizationInput input)
    {
        return _openPlatformAppService.CancelVehicleAuthorizationAsync(input);
    }

    [HttpGet("authorized")]
    public Task<OpenPlatformResponseDto<OpenPlatformAuthorizedDto>> GetAuthorizedAsync([FromQuery] GetVehicleControlAuthorizationInput input)
    {
        return _openPlatformAppService.GetAuthorizedAsync(input);
    }

    [HttpGet("vehicle-info")]
    public Task<OpenPlatformResponseDto<OpenPlatformVehicleInfoDto>> GetVehicleCurrentInfoAsync([FromQuery] GetVehicleControlAuthorizationInput input)
    {
        return _openPlatformAppService.GetVehicleCurrentInfoAsync(input);
    }

    [HttpGet("trip-list")]
    [HttpGet("vehicle-trips")]
    public Task<OpenPlatformResponseDto<OpenPlatformTripListDto>> GetVehicleTripsAsync([FromQuery] GetOpenPlatformVehicleTripsInput input)
    {
        return _openPlatformAppService.GetVehicleTripsAsync(input);
    }

    [HttpGet("trip-track")]
    [HttpGet("vehicle-traces")]
    public Task<OpenPlatformResponseDto<OpenPlatformTripTrackDto>> GetVehicleTraceAsync([FromQuery] GetOpenPlatformVehicleTraceInput input)
    {
        return _openPlatformAppService.GetVehicleTraceAsync(input);
    }

    [HttpPost("control")]
    public Task<OpenPlatformResponseDto<OpenPlatformControlResultDto>> ControlAsync([FromBody] OpenPlatformControlInput input)
    {
        return _openPlatformAppService.ControlAsync(input);
    }

    [HttpGet("alarms")]
    public Task<OpenPlatformResponseDto<OpenPlatformAlarmListDto>> GetAlarmsAsync([FromQuery] GetOpenPlatformAlarmsInput input)
    {
        return _openPlatformAppService.GetAlarmsAsync(input);
    }

    [HttpPost("enable-start")]
    public Task<OpenPlatformResponseDto<OpenPlatformCapabilityResultDto>> EnableStartAsync([FromBody] OpenPlatformVehicleCapabilityInput input)
    {
        return _openPlatformAppService.EnableStartAsync(input);
    }

    [HttpPost("disable-start")]
    public Task<OpenPlatformResponseDto<OpenPlatformCapabilityResultDto>> DisableStartAsync([FromBody] OpenPlatformVehicleCapabilityInput input)
    {
        return _openPlatformAppService.DisableStartAsync(input);
    }
}
