using Volo.Abp.Application.Dtos;

namespace CheMa.VNext.OpenPlatform.Inputs;

public class GetOpenAppListInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }

    public OpenAppStatus? Status { get; set; }
}
