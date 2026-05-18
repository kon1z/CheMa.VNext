using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace CheMa.VNext.Vehicles;

public class VehicleAppService : VNextAppService, IVehicleAppService
{
    private readonly IRepository<Vehicle, Guid> _vehicleRepository;
    private readonly IVehicleRepository _customVehicleRepository;

    public VehicleAppService(
        IRepository<Vehicle, Guid> vehicleRepository,
        IVehicleRepository customVehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
        _customVehicleRepository = customVehicleRepository;
    }

    public async Task<VehicleDto> GetAsync(Guid id)
    {
        var vehicle = await _vehicleRepository.GetAsync(id);
        return MapToVehicleDto(vehicle);
    }

    public async Task<PagedResultDto<VehicleDto>> GetListAsync(GetVehicleListInput input)
    {
        var queryable = await _vehicleRepository.GetQueryableAsync();

        var query = queryable
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(), x => x.Vin.Contains(input.Filter!) || (x.PlateNumber != null && x.PlateNumber.Contains(input.Filter!)));

        var totalCount = await AsyncExecuter.CountAsync(query);

        query = query
            .OrderBy(input.Sorting.IsNullOrWhiteSpace() ? nameof(Vehicle.CreationTime) + " desc" : input.Sorting)
            .PageBy(input);

        var items = await AsyncExecuter.ToListAsync(query);

        return new PagedResultDto<VehicleDto>(
            totalCount,
            items.Select(MapToVehicleDto).ToList());
    }

    public async Task<VehicleDto> CreateAsync(CreateVehicleDto input)
    {
        await EnsureVinUniqueAsync(input.Vin);

        var vehicle = new Vehicle(
            GuidGenerator.Create(),
            input.Vin,
            input.PlateNumber,
            input.DeviceType,
            input.BindingStatus,
            input.BindingTime);

        vehicle = await _vehicleRepository.InsertAsync(vehicle, autoSave: true);
        return MapToVehicleDto(vehicle);
    }

    public async Task<VehicleDto> UpdateAsync(Guid id, UpdateVehicleDto input)
    {
        var vehicle = await _vehicleRepository.GetAsync(id);

        await EnsureVinUniqueAsync(input.Vin, id);

        vehicle.SetVin(input.Vin);
        vehicle.SetPlateNumber(input.PlateNumber);
        vehicle.SetBindingInfo(input.DeviceType, input.BindingStatus, input.BindingTime);

        vehicle = await _vehicleRepository.UpdateAsync(vehicle, autoSave: true);
        return MapToVehicleDto(vehicle);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _vehicleRepository.DeleteAsync(id);
    }

    private async Task EnsureVinUniqueAsync(string vin, Guid? excludeId = null)
    {
        if (await _customVehicleRepository.ExistsByVinAsync(vin, excludeId))
        {
            throw new VehicleVinAlreadyExistsException(vin);
        }
    }

    private static VehicleDto MapToVehicleDto(Vehicle vehicle)
    {
        return new VehicleDto
        {
            Id = vehicle.Id,
            Vin = vehicle.Vin,
            PlateNumber = vehicle.PlateNumber,
            DeviceType = vehicle.DeviceType,
            BindingStatus = vehicle.BindingStatus,
            BindingTime = vehicle.BindingTime,
            CreationTime = vehicle.CreationTime,
            CreatorId = vehicle.CreatorId,
            LastModificationTime = vehicle.LastModificationTime,
            LastModifierId = vehicle.LastModifierId,
            IsDeleted = vehicle.IsDeleted,
            DeleterId = vehicle.DeleterId,
            DeletionTime = vehicle.DeletionTime
        };
    }
}
