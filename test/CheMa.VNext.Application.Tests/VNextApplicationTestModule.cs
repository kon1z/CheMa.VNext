using CheMa.VNext.OpenPlatform;
using CheMa.VNext.VehicleDevices;
using CheMa.VNext.VehicleDevices.Models;
using CheMa.VNext.Vehicles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;
using CheMa.VNext.OpenPlatform.AppServices;
using CheMa.VNext.OpenPlatform.Dtos;
using CheMa.VNext.VehicleDevices.Interfaces;
using Volo.Abp.Modularity;

namespace CheMa.VNext;

[DependsOn(
    typeof(VNextApplicationModule),
    typeof(VNextDomainTestModule)
)]
public class VNextApplicationTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.Replace(ServiceDescriptor.Singleton<IOpenPlatformRequestContextAccessor, TestOpenPlatformRequestContextAccessor>());
        context.Services.Replace(ServiceDescriptor.Singleton<IVehicleDeviceService, FakeVehicleDeviceService>());
    }

    public class TestOpenPlatformRequestContextAccessor : IOpenPlatformRequestContextAccessor
    {
        public OpenPlatformRequestContext? Current { get; set; }
    }

    private class FakeVehicleDeviceService : IVehicleDeviceService
    {
        public Task<BindVehicleDeviceResult> BindAsync(BindVehicleDeviceCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<UnbindVehicleDeviceResult> UnbindAsync(UnbindVehicleDeviceCommand command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<VehicleDeviceLocationResult> GetLocationAsync(Guid vehicleId, CancellationToken cancellationToken = default)
            => Task.FromResult(new VehicleDeviceLocationResult
            {
                VehicleId = vehicleId,
                VendorType = VehicleDeviceVendorType.MaiHong,
                VendorDeviceId = "MH-DEVICE-001",
                Latitude = 31.2304m,
                Longitude = 121.4737m,
                Speed = 68.5m,
                Direction = 135m,
                LocatedAtUtc = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });

        public Task<VehicleDeviceTrackResult> GetTrackAsync(VehicleDeviceTrackQuery query, CancellationToken cancellationToken = default)
            => Task.FromResult(new VehicleDeviceTrackResult
            {
                VehicleId = query.VehicleId,
                Points =
                [
                    new VehicleDeviceTrackPoint
                    {
                        Longitude = 121.4737m,
                        Latitude = 31.2304m,
                        LocatedAtUtc = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    }
                ]
            });

        public Task<VehicleDeviceTripResult> GetTripsAsync(VehicleDeviceTripQuery query, CancellationToken cancellationToken = default)
            => Task.FromResult(new VehicleDeviceTripResult
            {
                VehicleId = query.VehicleId,
                Trips =
                [
                    new VehicleDeviceTrip
                    {
                        TripId = "TRIP-001",
                        StartTimeUtc = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                        EndTimeUtc = new DateTime(2025, 1, 1, 1, 0, 0, DateTimeKind.Utc),
                        Mileage = 56.8m,
                        DurationSeconds = 3600,
                        AverageSpeed = 56.8m,
                        MaxSpeed = 88.8m
                    }
                ]
            });

        public Task<VehicleDeviceStatusResult> GetStatusAsync(Guid vehicleId, CancellationToken cancellationToken = default)
            => Task.FromResult(new VehicleDeviceStatusResult
            {
                VehicleId = vehicleId,
                VendorType = VehicleDeviceVendorType.MaiHong,
                VendorDeviceId = "MH-DEVICE-001",
                StatusTimeUtc = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Basic = new VehicleDeviceBasicStatus
                {
                    Online = true,
                    AccOn = true,
                    EngineOn = true,
                    Speed = 68.5m,
                    Mileage = 12345.6m,
                    FuelLevelPercent = 52.3m,
                    BatteryLevelPercent = 80m,
                    BatteryVoltage = 12.6m
                },
                Body = new VehicleDeviceBodyStatus
                {
                    Locked = true,
                    LeftFrontDoorOpen = false,
                    RightFrontDoorOpen = true,
                    LeftRearDoorOpen = false,
                    RightRearDoorOpen = true,
                    TrunkOpen = false,
                    HoodOpen = true,
                    WindowOpen = false,
                    DefendOn = true
                },
                Alert = new VehicleDeviceAlertStatus
                {
                    HasAlert = false,
                    Alerts = []
                }
            });

        public Task<VehicleDeviceControlResult> ControlAsync(VehicleDeviceControlCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(new VehicleDeviceControlResult
            {
                VehicleId = command.VehicleId,
                Action = command.Action,
                Success = command.Action is not VehicleDeviceControlAction.EnableStart and not VehicleDeviceControlAction.DisableStart,
                Message = command.Action is VehicleDeviceControlAction.EnableStart or VehicleDeviceControlAction.DisableStart
                    ? "Capability not supported by fake provider."
                    : "OK",
                VendorType = VehicleDeviceVendorType.MaiHong,
                VendorDeviceId = "MH-DEVICE-001"
            });

        public Task<VehicleDeviceAlertResult> GetAlertsAsync(VehicleDeviceAlertQuery query, CancellationToken cancellationToken = default)
            => Task.FromResult(new VehicleDeviceAlertResult
            {
                VehicleId = query.VehicleId,
                Alerts =
                [
                    new VehicleDeviceAlertItem
                    {
                        Code = "alarm-demo",
                        Message = "demo alarm",
                        Level = VehicleDeviceAlertLevel.Warning,
                        AlertTimeUtc = new DateTime(2025, 1, 1, 0, 30, 0, DateTimeKind.Utc)
                    }
                ]
            });
    }
}
