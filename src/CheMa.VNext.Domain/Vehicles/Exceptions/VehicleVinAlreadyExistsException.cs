using CheMa.VNext.Vehicles.Entities;
using Volo.Abp;

namespace CheMa.VNext.Vehicles.Exceptions;

public class VehicleVinAlreadyExistsException : BusinessException
{
    public VehicleVinAlreadyExistsException(string vin)
        : base(VNextDomainErrorCodes.VehicleVinAlreadyExists)
    {
        WithData(nameof(Vehicle.Vin), vin);
    }
}
