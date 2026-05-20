using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CheMa.VNext.VehicleDevices.AppServices;
using CheMa.VNext.VehicleDevices.Dtos;
using CheMa.VNext.VehicleDevices.Inputs;
using Microsoft.AspNetCore.Mvc;

namespace CheMa.VNext.Controllers.VehicleDevices;

[Route("api/vehicle-devices")]
public class VehicleDeviceController : VNextController
{
    private readonly IVehicleDeviceAppService _vehicleDeviceAppService;

    public VehicleDeviceController(IVehicleDeviceAppService vehicleDeviceAppService)
    {
        _vehicleDeviceAppService = vehicleDeviceAppService;
    }

    [HttpGet("brands")]
    public Task<List<VehicleBrandDto>> GetBrandsAsync()
    {
        return _vehicleDeviceAppService.GetBrandsAsync();
    }

    [HttpGet("styles")]
    public Task<List<VehicleStyleDto>> GetStylesAsync([FromQuery] string brandId)
    {
        return _vehicleDeviceAppService.GetStylesAsync(brandId);
    }

    [HttpGet("models")]
    public Task<List<VehicleModelDto>> GetModelsAsync([FromQuery] string styleId)
    {
        return _vehicleDeviceAppService.GetModelsAsync(styleId);
    }

    [HttpPost("bind")]
    public Task<BindVehicleDeviceDto> BindAsync([FromBody] BindVehicleDeviceInput input)
    {
        return _vehicleDeviceAppService.BindAsync(input);
    }

    [HttpPost("{vehicleId:guid}/unbind")]
    public Task<UnbindVehicleDeviceDto> UnbindAsync(Guid vehicleId)
    {
        return _vehicleDeviceAppService.UnbindAsync(vehicleId);
    }

    [HttpGet("{vehicleId:guid}/location")]
    public Task<VehicleDeviceLocationDto> GetLocationAsync(Guid vehicleId)
    {
        return _vehicleDeviceAppService.GetLocationAsync(vehicleId);
    }

    [HttpGet("{vehicleId:guid}/status")]
    public Task<VehicleDeviceStatusDto> GetStatusAsync(Guid vehicleId)
    {
        return _vehicleDeviceAppService.GetStatusAsync(vehicleId);
    }

    [HttpGet("track")]
    public Task<VehicleDeviceTrackDto> GetTrackAsync([FromQuery] GetVehicleDeviceTrackInput input)
    {
        return _vehicleDeviceAppService.GetTrackAsync(input);
    }

    [HttpPost("control")]
    public Task<VehicleDeviceControlDto> ControlAsync([FromBody] VehicleDeviceControlInput input)
    {
        return _vehicleDeviceAppService.ControlAsync(input);
    }
}
