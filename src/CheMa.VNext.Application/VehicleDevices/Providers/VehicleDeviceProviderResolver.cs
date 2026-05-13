using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace CheMa.VNext.VehicleDevices;

public class VehicleDeviceProviderResolver : IVehicleDeviceProviderResolver, ITransientDependency
{
    private readonly IReadOnlyDictionary<string, IVehicleDeviceProvider> providers;

    public VehicleDeviceProviderResolver(IEnumerable<IVehicleDeviceProvider> providers)
    {
        this.providers = providers.ToDictionary(
            x => x.Brand,
            StringComparer.OrdinalIgnoreCase);
    }

    public IVehicleDeviceProvider Resolve(string brand)
    {
        if (providers.TryGetValue(brand, out var provider))
        {
            return provider;
        }

        throw new BusinessException(VehicleDeviceErrorCodes.UnsupportedBrand)
            .WithData("Brand", brand);
    }
}
