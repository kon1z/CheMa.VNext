using System.Threading;
using System.Threading.Tasks;
using CheMa.VNext.VehicleDevices.Enums;
using CheMa.VNext.VehicleDevices.Models;
using CheMa.VNext.Vehicles.Enums;

namespace CheMa.VNext.VehicleDevices.Providers;

public interface IVehicleDeviceProvider
{
    VehicleDeviceVendorType VendorType { get; }

    bool SupportsControlAction(VehicleDeviceControlAction action);

    Task<VehicleDeviceBindingResult> BindAsync(
        VehicleDeviceBindingContext context,
        CancellationToken cancellationToken = default);

    Task UnbindAsync(
        VehicleDeviceOperationContext context,
        CancellationToken cancellationToken = default);

    Task<VehicleDeviceLocationResult> GetLocationAsync(
        VehicleDeviceOperationContext context,
        CancellationToken cancellationToken = default);

    Task<VehicleDeviceTrackResult> GetTrackAsync(
        VehicleDeviceOperationContext context,
        VehicleDeviceTrackQuery query,
        CancellationToken cancellationToken = default);

    Task<VehicleDeviceTripResult> GetTripsAsync(
        VehicleDeviceOperationContext context,
        VehicleDeviceTripQuery query,
        CancellationToken cancellationToken = default);

    Task<VehicleDeviceStatusResult> GetStatusAsync(
        VehicleDeviceOperationContext context,
        CancellationToken cancellationToken = default);

    Task<VehicleDeviceControlResult> ControlAsync(
        VehicleDeviceOperationContext context,
        VehicleDeviceControlAction action,
        CancellationToken cancellationToken = default);

    Task<VehicleDeviceAlertResult> GetAlertsAsync(
        VehicleDeviceOperationContext context,
        VehicleDeviceAlertQuery query,
        CancellationToken cancellationToken = default);
}
