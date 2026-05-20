using System;
using System.Threading.Tasks;
using CheMa.VNext.VehicleDevices;
using CheMa.VNext.VehicleDevices.Interfaces;
using CheMa.VNext.VehicleDevices.Models;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace CheMa.VNext.VehicleDevices.Services;

public class VehicleDeviceServiceTests : VNextApplicationTestBase<VNextApplicationTestModule>
{
    private readonly IVehicleDeviceService _vehicleDeviceService;

    public VehicleDeviceServiceTests()
    {
        _vehicleDeviceService = GetRequiredService<IVehicleDeviceService>();
    }

    [Fact]
    public async Task GetTripsAsync_Should_Throw_When_TimeRange_Exceeds_MaxHours()
    {
        var exception = await Should.ThrowAsync<BusinessException>(() =>
            _vehicleDeviceService.GetTripsAsync(new VehicleDeviceTripQuery
            {
                VehicleId = Guid.NewGuid(),
                StartTimeUtc = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                EndTimeUtc = new DateTime(2025, 1, 2, 1, 0, 0, DateTimeKind.Utc)
            }));

        exception.Code.ShouldBe(VehicleDeviceErrorCodes.InvalidTrackTimeRange);
    }
}
