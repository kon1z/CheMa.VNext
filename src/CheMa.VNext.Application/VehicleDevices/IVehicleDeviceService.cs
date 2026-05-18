using System;
using System.Threading;
using System.Threading.Tasks;
using CheMa.VNext.VehicleDevices.Models;

namespace CheMa.VNext.VehicleDevices;

public interface IVehicleDeviceService
{
    Task<BindVehicleDeviceResult> BindAsync(
        BindVehicleDeviceCommand command,
        CancellationToken cancellationToken = default);

    Task<UnbindVehicleDeviceResult> UnbindAsync(
        UnbindVehicleDeviceCommand command,
        CancellationToken cancellationToken = default);

    Task<VehicleDeviceLocationResult> GetLocationAsync(
        Guid vehicleId,
        CancellationToken cancellationToken = default);

    Task<VehicleDeviceTrackResult> GetTrackAsync(
        VehicleDeviceTrackQuery query,
        CancellationToken cancellationToken = default);

    Task<VehicleDeviceStatusResult> GetStatusAsync(
        Guid vehicleId,
        CancellationToken cancellationToken = default);

    Task<VehicleDeviceControlResult> ControlAsync(
        VehicleDeviceControlCommand command,
        CancellationToken cancellationToken = default);
}
