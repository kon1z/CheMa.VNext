using CheMa.VNext.Vehicles.Enums;

namespace CheMa.VNext.VehicleDevices.Providers;

public interface IVehicleDeviceProviderResolver
{
    IVehicleDeviceProvider Resolve(VehicleDeviceVendorType vendorType);
}
