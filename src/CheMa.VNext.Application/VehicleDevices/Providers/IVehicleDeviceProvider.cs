using System.Threading;
using System.Threading.Tasks;
using CheMa.VNext.VehicleDevices.Models;
using CheMa.VNext.Vehicles;

namespace CheMa.VNext.VehicleDevices.Providers;

public interface IVehicleDeviceProvider
{
    VehicleDeviceVendorType VendorType { get; }

    bool SupportsControlAction(VehicleDeviceControlAction action);

    Task BindAsync(
        VehicleDeviceBindingContext context,
        CancellationToken cancellationToken = default);

    Task UnbindAsync(
        VehicleDeviceBindingContext context,
        CancellationToken cancellationToken = default);

    Task<VehicleDeviceLocationResult> GetLocationAsync(
        VehicleDeviceContext context,
        CancellationToken cancellationToken = default);

    Task<VehicleDeviceTrackResult> GetTrackAsync(
        VehicleDeviceContext context,
        VehicleDeviceTrackQuery query,
        CancellationToken cancellationToken = default);

    Task<VehicleDeviceStatusResult> GetStatusAsync(
        VehicleDeviceContext context,
        CancellationToken cancellationToken = default);

    Task<VehicleDeviceControlResult> ControlAsync(
        VehicleDeviceContext context,
        VehicleDeviceControlAction action,
        CancellationToken cancellationToken = default);
}
