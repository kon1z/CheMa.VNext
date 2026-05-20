using System;
using System.Threading.Tasks;
using CheMa.VNext.VehicleDevices.Models;

namespace CheMa.VNext.VehicleCapabilities.Shared;

public interface IVehicleCapabilityOrchestrator
{
    Task<VehicleCapabilityResultDto> GetVehicleInfoAsync(VehicleAccessContext context, Guid vehicleId);

    Task<VehicleCapabilityResultDto> GetVehicleStatusAsync(VehicleAccessContext context, Guid vehicleId);

    Task<VehicleCapabilityResultDto> GetVehicleLocationAsync(VehicleAccessContext context, Guid vehicleId);

    Task<VehicleDeviceTripResult> GetVehicleTripsAsync(
        VehicleAccessContext context,
        VehicleDeviceTripQuery query);

    Task<VehicleDeviceTrackResult> GetVehicleTrackAsync(
        VehicleAccessContext context,
        VehicleDeviceTrackQuery query);

    Task<VehicleDeviceAlertResult> GetVehicleAlertsAsync(
        VehicleAccessContext context,
        VehicleDeviceAlertQuery query);
}
