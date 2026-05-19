using System;
using System.Threading.Tasks;
using CheMa.VNext.Vehicles;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace CheMa.VNext.OpenPlatform;

public class VehicleControlAuthorizationAppService : VNextAppService, IVehicleControlAuthorizationAppService
{
    private readonly IRepository<OpenApp, Guid> _openAppRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IVehicleControlAuthorizationRepository _vehicleControlAuthorizationRepository;
    private readonly IOpenPlatformRequestContextAccessor _openPlatformRequestContextAccessor;

    public VehicleControlAuthorizationAppService(
        IRepository<OpenApp, Guid> openAppRepository,
        IVehicleRepository vehicleRepository,
        IVehicleControlAuthorizationRepository vehicleControlAuthorizationRepository,
        IOpenPlatformRequestContextAccessor openPlatformRequestContextAccessor)
    {
        _openAppRepository = openAppRepository;
        _vehicleRepository = vehicleRepository;
        _vehicleControlAuthorizationRepository = vehicleControlAuthorizationRepository;
        _openPlatformRequestContextAccessor = openPlatformRequestContextAccessor;
    }

    public async Task<VehicleControlAuthorizationDto> GetAuthorizedAsync(GetVehicleControlAuthorizationInput input)
    {
        Check.NotNull(input, nameof(input));
        Check.NotNullOrWhiteSpace(input.Vin, nameof(input.Vin), VehicleConsts.MaxVinLength);

        var clientId = _openPlatformRequestContextAccessor.Current?.ClientId;
        Check.NotNullOrWhiteSpace(clientId, nameof(OpenPlatformRequestContext.ClientId), OpenPlatformConsts.MaxClientIdLength);

        var openApp = await _openAppRepository.FirstOrDefaultAsync(x => x.ClientId == clientId);
        if (openApp == null)
        {
            throw new BusinessException("VNext:OpenPlatform:ClientIdNotFound")
                .WithData(nameof(clientId), clientId);
        }

        var vehicle = await _vehicleRepository.FirstOrDefaultAsync(x => x.Vin == input.Vin);
        if (vehicle == null)
        {
            throw new BusinessException("VNext:OpenPlatform:VehicleNotFound")
                .WithData(nameof(input.Vin), input.Vin);
        }

        var authorization = await _vehicleControlAuthorizationRepository.FindCurrentByOpenAppIdAndVinAsync(openApp.Id, input.Vin, Clock.Now);
        if (authorization == null)
        {
            throw new BusinessException("VNext:OpenPlatform:VehicleControlAuthorizationNotFound")
                .WithData(nameof(openApp.Id), openApp.Id)
                .WithData(nameof(input.Vin), input.Vin);
        }

        return new VehicleControlAuthorizationDto
        {
            OpenAppId = authorization.OpenAppId,
            VehicleVin = authorization.VehicleVin,
            DeviceVin = authorization.DeviceVin,
            VendorDeviceId = authorization.VendorDeviceId,
            AuthorizationStartTime = authorization.AuthorizationStartTime,
            AuthorizationEndTime = authorization.AuthorizationEndTime
        };
    }
}
