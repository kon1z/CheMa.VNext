using System;
using System.Threading.Tasks;
using CheMa.VNext.OpenPlatform;
using CheMa.VNext.OpenPlatform.AppServices;
using CheMa.VNext.OpenPlatform.Dtos;
using CheMa.VNext.OpenPlatform.Entities;
using CheMa.VNext.OpenPlatform.Inputs;
using CheMa.VNext.OpenPlatform.Repositories;
using CheMa.VNext.VehicleDevices;
using CheMa.VNext.VehicleDevices.Entities;
using CheMa.VNext.VehicleDevices.Repositories;
using CheMa.VNext.Vehicles;
using CheMa.VNext.Vehicles.Entities;
using CheMa.VNext.Vehicles.Repositories;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace CheMa.VNext.OpenPlatform;

public class VehicleControlAuthorizationAppService_Tests : VNextApplicationTestBase<VNextApplicationTestModule>
{
    private readonly IVehicleControlAuthorizationAppService _vehicleControlAuthorizationAppService;
    private readonly IRepository<OpenApp, Guid> _openAppRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IVehicleControlAuthorizationRepository _vehicleControlAuthorizationRepository;
    private readonly IVehicleDeviceRepository _vehicleDeviceRepository;
    private readonly VNextApplicationTestModule.TestOpenPlatformRequestContextAccessor _requestContextAccessor;

    public VehicleControlAuthorizationAppService_Tests()
    {
        _vehicleControlAuthorizationAppService = GetRequiredService<IVehicleControlAuthorizationAppService>();
        _openAppRepository = GetRequiredService<IRepository<OpenApp, Guid>>();
        _vehicleRepository = GetRequiredService<IVehicleRepository>();
        _vehicleControlAuthorizationRepository = GetRequiredService<IVehicleControlAuthorizationRepository>();
        _vehicleDeviceRepository = GetRequiredService<IVehicleDeviceRepository>();
        _requestContextAccessor = GetRequiredService<VNextApplicationTestModule.TestOpenPlatformRequestContextAccessor>();
    }

    [Fact]
    public async Task Should_Get_Vehicle_Current_Info_When_Authorized_And_Bound()
    {
        var openApp = await _openAppRepository.InsertAsync(new OpenApp(
            Guid.NewGuid(),
            "Current Info App",
            "client-current-info",
            "secret-cipher",
            "****ret"), autoSave: true);
        var vehicle = await _vehicleRepository.InsertAsync(new Vehicle(
            Guid.NewGuid(),
            "LSV12345678901234",
            null,
            null,
            VehicleBindingStatus.Unbound,
            null), autoSave: true);
        var vehicleDevice = new VehicleDevice(Guid.NewGuid(), VehicleDeviceVendorType.MaiHong, "MH-DEVICE-001");
        vehicleDevice.Bind(vehicle.Id);
        await _vehicleDeviceRepository.InsertAsync(vehicleDevice, autoSave: true);
        vehicle.SetBindingInfo(VehicleDeviceVendorType.MaiHong, VehicleBindingStatus.Bound, DateTime.UtcNow);
        await _vehicleRepository.UpdateAsync(vehicle, autoSave: true);

        await _vehicleControlAuthorizationRepository.InsertAsync(new VehicleControlAuthorization(
            Guid.NewGuid(),
            openApp.Id,
            vehicle.Id,
            vehicleDevice.Id,
            vehicle.Vin,
            vehicle.Vin,
            vehicleDevice.VendorDeviceId,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1)), autoSave: true);

        _requestContextAccessor.Current = new OpenPlatformRequestContext
        {
            ClientId = openApp.ClientId
        };

        var result = await _vehicleControlAuthorizationAppService.GetVehicleCurrentInfoAsync(new GetVehicleControlAuthorizationInput
        {
            Vin = vehicle.Vin
        });

        result.Vin.ShouldBe(vehicle.Vin);
        result.Lat.ShouldBe(31.2304m);
        result.Lon.ShouldBe(121.4737m);
        result.Speed.ShouldBe(68.5m);
        result.Direction.ShouldBe(135m);
        result.Engine.ShouldBe(1);
        result.LockStatus.ShouldBe(1);
        result.AccStatus.ShouldBe(1);
        result.OriginalMileage.ShouldBe(12345.6m);
        result.FuelLevel.ShouldBe(52.3m);
        result.Soc.ShouldBe(80m);
        result.CarVoltage.ShouldBe(12.6m);
        result.Door1Status.ShouldBe(0);
        result.Door2Status.ShouldBe(1);
        result.Door3Status.ShouldBe(0);
        result.Door4Status.ShouldBe(1);
        result.Door5Status.ShouldBe(0);
        result.Bonnet.ShouldBe(1);
        result.LocationStatus.ShouldBe(1);
        result.CarFortificationStatus.ShouldBe(1);
        result.ReportTime.ShouldBe(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        result.SendTime.ShouldBeNull();
        result.ReceiveTime.ShouldBeNull();
    }

    [Fact]
    public async Task Should_Throw_When_Vehicle_Device_Is_Not_Bound()
    {
        var openApp = await _openAppRepository.InsertAsync(new OpenApp(
            Guid.NewGuid(),
            "Unbound App",
            "client-unbound",
            "secret-cipher",
            "****ret"), autoSave: true);
        var vehicle = await _vehicleRepository.InsertAsync(new Vehicle(
            Guid.NewGuid(),
            "LSV12345678909999",
            null,
            null,
            VehicleBindingStatus.Unbound,
            null), autoSave: true);

        await _vehicleControlAuthorizationRepository.InsertAsync(new VehicleControlAuthorization(
            Guid.NewGuid(),
            openApp.Id,
            vehicle.Id,
            Guid.NewGuid(),
            vehicle.Vin,
            vehicle.Vin,
            "UNKNOWN-DEVICE",
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1)), autoSave: true);

        _requestContextAccessor.Current = new OpenPlatformRequestContext
        {
            ClientId = openApp.ClientId
        };

        var exception = await Should.ThrowAsync<BusinessException>(async () =>
            await _vehicleControlAuthorizationAppService.GetVehicleCurrentInfoAsync(new GetVehicleControlAuthorizationInput
            {
                Vin = vehicle.Vin
            }));

        exception.Code.ShouldBe(VehicleDeviceErrorCodes.BindingNotFound);
    }
}
