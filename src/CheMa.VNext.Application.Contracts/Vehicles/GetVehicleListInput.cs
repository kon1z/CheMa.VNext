using Volo.Abp.Application.Dtos;

namespace CheMa.VNext.Vehicles;

public class GetVehicleListInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}
