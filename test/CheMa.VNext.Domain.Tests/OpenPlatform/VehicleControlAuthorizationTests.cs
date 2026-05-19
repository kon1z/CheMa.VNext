using System;
using CheMa.VNext.OpenPlatform;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace CheMa.VNext.OpenPlatform;

public class VehicleControlAuthorizationTests : VNextDomainTestBase<VNextDomainTestModule>
{
    [Fact]
    public void Should_Update_Device_And_Period_When_Renewing()
    {
        var authorization = new VehicleControlAuthorization(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "VIN001",
            "VIN001",
            "DEVICE-001",
            new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 1, 31, 0, 0, 0, DateTimeKind.Utc));
        var newVehicleDeviceId = Guid.NewGuid();
        var startTime = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        var endTime = new DateTime(2025, 2, 28, 0, 0, 0, DateTimeKind.Utc);

        authorization.Renew(newVehicleDeviceId, "VIN002", "VIN002", "DEVICE-002", startTime, endTime);

        authorization.VehicleDeviceId.ShouldBe(newVehicleDeviceId);
        authorization.VehicleVin.ShouldBe("VIN002");
        authorization.DeviceVin.ShouldBe("VIN002");
        authorization.VendorDeviceId.ShouldBe("DEVICE-002");
        authorization.AuthorizationStartTime.ShouldBe(startTime);
        authorization.AuthorizationEndTime.ShouldBe(endTime);
    }

    [Fact]
    public void Should_Cancel_By_Updating_End_Time()
    {
        var startTime = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var authorization = new VehicleControlAuthorization(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "VIN001",
            "VIN001",
            "DEVICE-001",
            startTime,
            new DateTime(2025, 1, 31, 0, 0, 0, DateTimeKind.Utc));
        var cancelTime = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc);

        authorization.Cancel(cancelTime);

        authorization.AuthorizationEndTime.ShouldBe(cancelTime);
    }

    [Fact]
    public void Should_Throw_When_Cancel_Time_Is_Earlier_Than_Start_Time()
    {
        var startTime = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        var authorization = new VehicleControlAuthorization(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "VIN001",
            "VIN001",
            "DEVICE-001",
            startTime,
            new DateTime(2025, 1, 31, 0, 0, 0, DateTimeKind.Utc));

        Should.Throw<BusinessException>(() => authorization.Cancel(startTime.AddDays(-1)));
    }
}
