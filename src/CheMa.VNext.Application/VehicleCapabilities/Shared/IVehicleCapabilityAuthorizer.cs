using System.Threading.Tasks;

namespace CheMa.VNext.VehicleCapabilities.Shared;

public interface IVehicleCapabilityAuthorizer
{
    Task AuthorizeAsync(VehicleAccessContext context, VehicleCapabilityRequirement requirement);
}
