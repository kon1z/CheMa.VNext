namespace CheMa.VNext.VehicleDevices.Providers;

public interface IVehicleDeviceProviderResolver
{
    IVehicleDeviceProvider Resolve(string brand);
}
