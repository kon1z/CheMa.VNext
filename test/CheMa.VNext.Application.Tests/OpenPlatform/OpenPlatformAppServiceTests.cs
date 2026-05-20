using System;
using System.Threading.Tasks;
using CheMa.VNext.OpenPlatform.AppServices;
using CheMa.VNext.OpenPlatform.Dtos;
using CheMa.VNext.OpenPlatform.Entities;
using CheMa.VNext.OpenPlatform.Repositories;
using CheMa.VNext.Vehicles;
using CheMa.VNext.Vehicles.Entities;
using CheMa.VNext.Vehicles.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace CheMa.VNext.OpenPlatform;

public class OpenPlatformAppServiceTests : VNextApplicationTestBase<VNextApplicationTestModule>
{
    private readonly IOpenPlatformAppService _openPlatformAppService;
    private readonly IRepository<OpenApp, Guid> _openAppRepository;
    private readonly IRepository<Vehicle, Guid> _vehicleRepository;
    private readonly IRepository<CheMa.VNext.VehicleDevices.Entities.VehicleDevice, Guid> _vehicleDeviceRepository;
    private readonly IVehicleControlAuthorizationRepository _authorizationRepository;
    private readonly VNextApplicationTestModule.TestOpenPlatformRequestContextAccessor _requestContextAccessor;

    public OpenPlatformAppServiceTests()
    {
        _openPlatformAppService = GetRequiredService<IOpenPlatformAppService>();
        _openAppRepository = GetRequiredService<IRepository<OpenApp, Guid>>();
        _vehicleRepository = GetRequiredService<IRepository<Vehicle, Guid>>();
        _vehicleDeviceRepository = GetRequiredService<IRepository<CheMa.VNext.VehicleDevices.Entities.VehicleDevice, Guid>>();
        _authorizationRepository = GetRequiredService<IVehicleControlAuthorizationRepository>();
        _requestContextAccessor = (VNextApplicationTestModule.TestOpenPlatformRequestContextAccessor)GetRequiredService<IOpenPlatformRequestContextAccessor>();
    }

    [Fact]
    public async Task Should_Return_Uniform_Response_For_Authorized_Query()
    {
        var context = await SeedAuthorizedVehicleAsync();

        var result = await _openPlatformAppService.GetAuthorizedAsync(new Inputs.GetVehicleControlAuthorizationInput
        {
            Vin = context.Vehicle.Vin
        });

        result.Code.ShouldBe(OpenPlatformResponseCodes.Success);
        result.Message.ShouldBe("success");
        result.TraceId.ShouldNotBeNull();
        result.Data.ShouldNotBeNull();
        result.Data.Vin.ShouldBe(context.Vehicle.Vin);
        result.Data.Authorized.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Return_Trip_List_And_Alarms()
    {
        var context = await SeedAuthorizedVehicleAsync();

        var trips = await _openPlatformAppService.GetVehicleTripsAsync(new Inputs.GetOpenPlatformVehicleTripsInput
        {
            Vin = context.Vehicle.Vin,
            StartTimeUtc = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            EndTimeUtc = new DateTime(2025, 1, 1, 2, 0, 0, DateTimeKind.Utc)
        });
        var alarms = await _openPlatformAppService.GetAlarmsAsync(new Inputs.GetOpenPlatformAlarmsInput
        {
            Vin = context.Vehicle.Vin
        });

        trips.Data.ShouldNotBeNull();
        trips.Data.TotalCount.ShouldBe(1);
        trips.Data.Items.Count.ShouldBe(1);
        alarms.Data.ShouldNotBeNull();
        alarms.Data.TotalCount.ShouldBe(1);
        alarms.Data.Items.Count.ShouldBe(1);
    }

    private async Task<(OpenApp OpenApp, Vehicle Vehicle)> SeedAuthorizedVehicleAsync()
    {
        var openApp = await _openAppRepository.InsertAsync(new OpenApp(
            Guid.NewGuid(),
            "demo-app",
            "client-001",
            "cipher",
            "hint",
            null,
            null,
            null,
            null), autoSave: true);

        var vehicle = await _vehicleRepository.InsertAsync(new Vehicle(
            Guid.NewGuid(),
            $"VIN-{Guid.NewGuid():N}",
            "沪A12345",
            "BYD",
            "Song",
            "Plus",
            VehicleDeviceVendorType.MaiHong,
            VehicleBindingStatus.Bound,
            DateTime.UtcNow), autoSave: true);

        var vehicleDevice = await _vehicleDeviceRepository.InsertAsync(new CheMa.VNext.VehicleDevices.Entities.VehicleDevice(
            Guid.NewGuid(),
            VehicleDeviceVendorType.MaiHong,
            "MH-DEVICE-001"), autoSave: true);
        vehicleDevice.Bind(vehicle.Id);
        await _vehicleDeviceRepository.UpdateAsync(vehicleDevice, autoSave: true);

        await _authorizationRepository.InsertAsync(new VehicleControlAuthorization(
            Guid.NewGuid(),
            openApp.Id,
            vehicle.Id,
            vehicleDevice.Id,
            vehicle.Vin,
            vehicle.Vin,
            vehicleDevice.VendorDeviceId,
            DateTime.UtcNow.AddHours(-1),
            DateTime.UtcNow.AddHours(1)), autoSave: true);

        _requestContextAccessor.Current = new OpenPlatformRequestContext
        {
            OpenAppId = openApp.Id,
            ClientId = openApp.ClientId,
            AppName = openApp.Name,
            RequestTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Nonce = Guid.NewGuid().ToString("N")
        };

        return (openApp, vehicle);
    }
}
