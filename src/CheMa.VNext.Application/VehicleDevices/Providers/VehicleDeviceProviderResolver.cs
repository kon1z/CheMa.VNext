using System.Collections.Generic;
using System.Linq;
using CheMa.VNext.Vehicles;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace CheMa.VNext.VehicleDevices.Providers;

public class VehicleDeviceProviderResolver : IVehicleDeviceProviderResolver, ITransientDependency
{
    private readonly IReadOnlyDictionary<VehicleDeviceVendorType, IVehicleDeviceProvider> _providers;

    public VehicleDeviceProviderResolver(IEnumerable<IVehicleDeviceProvider> providers)
    {
        _providers = providers.ToDictionary(x => x.VendorType);
    }

    public IVehicleDeviceProvider Resolve(VehicleDeviceVendorType vendorType)
    {
        if (_providers.TryGetValue(vendorType, out var provider))
        {
            return provider;
        }

        throw new BusinessException(VehicleDeviceErrorCodes.UnsupportedBrand)
            .WithData("VendorType", vendorType);
    }
}
