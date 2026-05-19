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

    [HttpGet("authorized")]
    public Task<VehicleControlAuthorizationDto> GetAuthorizedAsync([FromQuery] GetVehicleControlAuthorizationInput input)
    {
        return _openPlatformAppService.GetAuthorizedAsync(input);
    }

    [HttpGet("vehicle-info")]
    public Task<OpenPlatformVehicleCurrentInfoDto> GetVehicleCurrentInfoAsync([FromQuery] GetVehicleControlAuthorizationInput input)
    {
        return _openPlatformAppService.GetVehicleCurrentInfoAsync(input);
    }
}
