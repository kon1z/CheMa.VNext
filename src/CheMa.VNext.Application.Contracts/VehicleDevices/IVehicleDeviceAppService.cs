using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace CheMa.VNext.VehicleDevices;

public interface IVehicleDeviceAppService : IApplicationService
{
    Task<BindVehicleDeviceDto> BindAsync(BindVehicleDeviceInput input);

    Task<UnbindVehicleDeviceDto> UnbindAsync(Guid vehicleId);

    Task<VehicleDeviceLocationDto> GetLocationAsync(Guid vehicleId);

    Task<VehicleDeviceTrackDto> GetTrackAsync(GetVehicleDeviceTrackInput input);

    Task<VehicleDeviceStatusDto> GetStatusAsync(Guid vehicleId);

    Task<VehicleDeviceControlDto> ControlAsync(VehicleDeviceControlInput input);
}
