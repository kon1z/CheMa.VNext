using System;
using System.Threading.Tasks;
using CheMa.VNext.OpenPlatform;
using CheMa.VNext.OpenPlatform.AppServices;
using CheMa.VNext.OpenPlatform.Dtos;
using CheMa.VNext.OpenPlatform.Entities;
using CheMa.VNext.OpenPlatform.Inputs;
using CheMa.VNext.VehicleDevices;
using CheMa.VNext.VehicleDevices.Entities;
using CheMa.VNext.Vehicles;
using CheMa.VNext.Vehicles.Entities;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace CheMa.VNext.OpenPlatform;

public class OpenAppVehicleAuthorizationAppServiceTests : VNextApplicationTestBase<VNextApplicationTestModule>
{
    private readonly IOpenAppVehicleAuthorizationAppService _appService;
    private readonly IRepository<OpenApp, Guid> _openAppRepository;
    private readonly IRepository<Vehicle, Guid> _vehicleRepository;
    private readonly IRepository<VehicleDevice, Guid> _vehicleDeviceRepository;

    public OpenAppVehicleAuthorizationAppServiceTests()
    {
        _appService = GetRequiredService<IOpenAppVehicleAuthorizationAppService>();
        _openAppRepository = GetRequiredService<IRepository<OpenApp, Guid>>();
        _vehicleRepository = GetRequiredService<IRepository<Vehicle, Guid>>();
        _vehicleDeviceRepository = GetRequiredService<IRepository<VehicleDevice, Guid>>();
    }

    [Fact]
    public async Task Should_Authorize_Vehicle_With_Current_Device()
    {
        var openApp = await CreateOpenAppAsync("app-authorize");
        var vehicle = await CreateVehicleAsync("VIN-AUTH-001");
        var vehicleDevice = await CreateVehicleDeviceAsync(vehicle.Id, "DEVICE-AUTH-001");

        var result = await _appService.AuthorizeAsync(openApp.Id, new AuthorizeOpenAppVehicleDto
        {
            VehicleId = vehicle.Id,
            AuthorizationStartTime = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            AuthorizationEndTime = new DateTime(2025, 1, 31, 0, 0, 0, DateTimeKind.Utc)
        });

        result.OpenAppId.ShouldBe(openApp.Id);
        result.VehicleId.ShouldBe(vehicle.Id);
        result.VehicleDeviceId.ShouldBe(vehicleDevice.Id);
        result.VehicleVin.ShouldBe(vehicle.Vin);
        result.DeviceVin.ShouldBe(vehicle.Vin);
        result.VendorDeviceId.ShouldBe(vehicleDevice.VendorDeviceId);
    }

    [Fact]
    public async Task Should_Throw_When_Vehicle_Has_No_Current_Device()
    {
        var openApp = await CreateOpenAppAsync("app-no-device");
        var vehicle = await CreateVehicleAsync("VIN-NODEVICE-001");

        var exception = await Should.ThrowAsync<BusinessException>(() => _appService.AuthorizeAsync(openApp.Id, new AuthorizeOpenAppVehicleDto
        {
            VehicleId = vehicle.Id,
            AuthorizationStartTime = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            AuthorizationEndTime = new DateTime(2025, 1, 31, 0, 0, 0, DateTimeKind.Utc)
        }));

        exception.Code.ShouldBe("VNext:OpenPlatform:VehicleDeviceNotFound");
    }

    [Fact]
    public async Task Should_Throw_When_Other_Open_App_Conflicts()
    {
        var firstOpenApp = await CreateOpenAppAsync("app-conflict-1");
        var secondOpenApp = await CreateOpenAppAsync("app-conflict-2");
        var vehicle = await CreateVehicleAsync("VIN-CONFLICT-001");
        await CreateVehicleDeviceAsync(vehicle.Id, "DEVICE-CONFLICT-001");

        await _appService.AuthorizeAsync(firstOpenApp.Id, new AuthorizeOpenAppVehicleDto
        {
            VehicleId = vehicle.Id,
            AuthorizationStartTime = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            AuthorizationEndTime = new DateTime(2025, 1, 31, 0, 0, 0, DateTimeKind.Utc)
        });

        var exception = await Should.ThrowAsync<BusinessException>(() => _appService.AuthorizeAsync(secondOpenApp.Id, new AuthorizeOpenAppVehicleDto
        {
            VehicleId = vehicle.Id,
            AuthorizationStartTime = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            AuthorizationEndTime = new DateTime(2025, 2, 15, 0, 0, 0, DateTimeKind.Utc)
        }));

        exception.Code.ShouldBe("VNext:OpenPlatform:VehicleControlAuthorizationConflict");
    }

    [Fact]
    public async Task Should_Renew_And_Refresh_Device_Snapshot_For_Same_Open_App()
    {
        var openApp = await CreateOpenAppAsync("app-renew");
        var vehicle = await CreateVehicleAsync("VIN-RENEW-001");
        var oldDevice = await CreateVehicleDeviceAsync(vehicle.Id, "DEVICE-OLD-001");

        var authorization = await _appService.AuthorizeAsync(openApp.Id, new AuthorizeOpenAppVehicleDto
        {
            VehicleId = vehicle.Id,
            AuthorizationStartTime = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            AuthorizationEndTime = new DateTime(2025, 1, 31, 0, 0, 0, DateTimeKind.Utc)
        });

        await ReplaceVehicleDeviceAsync(oldDevice, vehicle.Id, "DEVICE-NEW-001");

        var renewed = await _appService.RenewAsync(authorization.Id, new RenewOpenAppVehicleAuthorizationDto
        {
            AuthorizationStartTime = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            AuthorizationEndTime = new DateTime(2025, 2, 28, 0, 0, 0, DateTimeKind.Utc)
        });

        renewed.VehicleDeviceId.ShouldNotBe(oldDevice.Id);
        renewed.VendorDeviceId.ShouldBe("DEVICE-NEW-001");
    }

    [Fact]
    public async Task Should_Cancel_And_Remove_Current_Authorization_Status()
    {
        var openApp = await CreateOpenAppAsync("app-cancel");
        var vehicle = await CreateVehicleAsync("VIN-CANCEL-001");
        await CreateVehicleDeviceAsync(vehicle.Id, "DEVICE-CANCEL-001");
        var queryService = GetRequiredService<IOpenPlatformAppService>();

        var now = DateTime.UtcNow;
        var authorization = await _appService.AuthorizeAsync(openApp.Id, new AuthorizeOpenAppVehicleDto
        {
            VehicleId = vehicle.Id,
            AuthorizationStartTime = now.AddDays(-1),
            AuthorizationEndTime = now.AddDays(1)
        });

        await _appService.CancelAsync(authorization.Id, new CancelOpenAppVehicleAuthorizationDto
        {
            CancelTime = now.AddMinutes(-1)
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var contextAccessor = GetRequiredService<IOpenPlatformRequestContextAccessor>();
            contextAccessor.Current = new OpenPlatformRequestContext
            {
                OpenAppId = openApp.Id,
                ClientId = openApp.ClientId,
                AppName = openApp.Name,
                RequestTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Nonce = Guid.NewGuid().ToString("N")
            };

            var exception = await Should.ThrowAsync<BusinessException>(() => queryService.GetAuthorizedAsync(new GetVehicleControlAuthorizationInput
            {
                Vin = vehicle.Vin
            }));

            exception.Code.ShouldBe("VNext:OpenPlatform:VehicleControlAuthorizationNotFound");
        });
    }

    private async Task<OpenApp> CreateOpenAppAsync(string name)
    {
        var openApp = new OpenApp(
            Guid.NewGuid(),
            name,
            $"client-{Guid.NewGuid():N}",
            "cipher",
            "hint",
            new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc));

        await WithUnitOfWorkAsync(() => _openAppRepository.InsertAsync(openApp, autoSave: true));
        return openApp;
    }

    private async Task<Vehicle> CreateVehicleAsync(string vin)
    {
        var vehicle = new Vehicle(
            Guid.NewGuid(),
            vin,
            null,
            VehicleDeviceVendorType.MaiHong,
            VehicleBindingStatus.Bound,
            new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        await WithUnitOfWorkAsync(() => _vehicleRepository.InsertAsync(vehicle, autoSave: true));
        return vehicle;
    }

    private async Task<VehicleDevice> CreateVehicleDeviceAsync(Guid vehicleId, string vendorDeviceId)
    {
        var vehicleDevice = new VehicleDevice(Guid.NewGuid(), VehicleDeviceVendorType.MaiHong, vendorDeviceId, vehicleId);
        await WithUnitOfWorkAsync(() => _vehicleDeviceRepository.InsertAsync(vehicleDevice, autoSave: true));
        return vehicleDevice;
    }

    private async Task ReplaceVehicleDeviceAsync(VehicleDevice oldDevice, Guid vehicleId, string newVendorDeviceId)
    {
        await WithUnitOfWorkAsync(async () =>
        {
            oldDevice.Unbind();
            await _vehicleDeviceRepository.UpdateAsync(oldDevice, autoSave: true);

            var newDevice = new VehicleDevice(Guid.NewGuid(), VehicleDeviceVendorType.MaiHong, newVendorDeviceId, vehicleId);
            await _vehicleDeviceRepository.InsertAsync(newDevice, autoSave: true);
        });
    }
}
