namespace CheMa.VNext.VehicleDevices;

public interface IVehicleDeviceProviderResolver
{
    IVehicleDeviceProvider Resolve(string brand);
}
