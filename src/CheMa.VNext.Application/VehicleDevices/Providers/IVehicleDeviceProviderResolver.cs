using CheMa.VNext.Vehicles;

namespace CheMa.VNext.VehicleDevices.Providers;

public interface IVehicleDeviceProviderResolver
{
    IVehicleDeviceProvider Resolve(VehicleDeviceVendorType vendorType);
}
