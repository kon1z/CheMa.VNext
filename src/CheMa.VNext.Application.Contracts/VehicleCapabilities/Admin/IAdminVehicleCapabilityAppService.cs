using System;
using System.Threading.Tasks;
using CheMa.VNext.VehicleCapabilities.Shared;
using Volo.Abp.Application.Services;

namespace CheMa.VNext.VehicleCapabilities.Admin;

public interface IAdminVehicleCapabilityAppService : IApplicationService
{
    Task<VehicleCapabilityResultDto> GetVehicleInfoAsync(Guid vehicleId);

    Task<VehicleCapabilityResultDto> GetVehicleStatusAsync(Guid vehicleId);

    Task<VehicleCapabilityResultDto> GetVehicleLocationAsync(Guid vehicleId);

    Task<AdminVehicleTripListDto> GetVehicleTripsAsync(AdminVehicleTripQueryDto input);

    Task<AdminVehicleTrackDto> GetVehicleTrackAsync(AdminVehicleTrackQueryDto input);

    Task<AdminVehicleAlertListDto> GetVehicleAlertsAsync(AdminVehicleAlertQueryDto input);
}
