using Volo.Abp;

namespace CheMa.VNext.Vehicles;

public class VehicleVinAlreadyExistsException : BusinessException
{
    public VehicleVinAlreadyExistsException(string vin)
        : base(VNextDomainErrorCodes.VehicleVinAlreadyExists)
    {
        WithData(nameof(Vehicle.Vin), vin);
    }
}
