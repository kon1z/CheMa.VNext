using System;
using System.Threading.Tasks;
using CheMa.VNext.Vehicles.Dtos;
using CheMa.VNext.Vehicles.Inputs;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace CheMa.VNext.Vehicles.AppServices;

public interface IVehicleAppService : IApplicationService
{
    Task<VehicleDto> GetAsync(Guid id);

    Task<PagedResultDto<VehicleDto>> GetListAsync(GetVehicleListInput input);

    Task<VehicleDto> CreateAsync(CreateVehicleDto input);

    Task<VehicleDto> UpdateAsync(Guid id, UpdateVehicleDto input);

    Task DeleteAsync(Guid id);
}
