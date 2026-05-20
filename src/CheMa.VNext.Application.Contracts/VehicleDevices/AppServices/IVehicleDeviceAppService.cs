using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CheMa.VNext.VehicleDevices.Dtos;
using CheMa.VNext.VehicleDevices.Inputs;
using Volo.Abp.Application.Services;

namespace CheMa.VNext.VehicleDevices.AppServices;

public interface IVehicleDeviceAppService : IApplicationService
{
    Task<List<VehicleBrandDto>> GetBrandsAsync();

    Task<List<VehicleStyleDto>> GetStylesAsync(string brandId);

    Task<List<VehicleModelDto>> GetModelsAsync(string styleId);

    Task<BindVehicleDeviceDto> BindAsync(BindVehicleDeviceInput input);

    Task<UnbindVehicleDeviceDto> UnbindAsync(Guid vehicleId);

    Task<VehicleDeviceLocationDto> GetLocationAsync(Guid vehicleId);

    Task<VehicleDeviceTrackDto> GetTrackAsync(GetVehicleDeviceTrackInput input);

    Task<VehicleDeviceStatusDto> GetStatusAsync(Guid vehicleId);

    Task<VehicleDeviceControlDto> ControlAsync(VehicleDeviceControlInput input);
}
