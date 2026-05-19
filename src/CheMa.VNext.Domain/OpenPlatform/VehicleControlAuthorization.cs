using System;
using CheMa.VNext.VehicleDevices;
using CheMa.VNext.Vehicles;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace CheMa.VNext.OpenPlatform;

public class VehicleControlAuthorization : FullAuditedAggregateRoot<Guid>
{
    public Guid OpenAppId { get; private set; }

    public Guid VehicleId { get; private set; }

    public Guid VehicleDeviceId { get; private set; }

    public string VehicleVin { get; private set; } = string.Empty;

    public string DeviceVin { get; private set; } = string.Empty;

    public string VendorDeviceId { get; private set; } = string.Empty;

    public DateTime AuthorizationStartTime { get; private set; }

    public DateTime? AuthorizationEndTime { get; private set; }

    protected VehicleControlAuthorization()
    {
    }

    public VehicleControlAuthorization(
        Guid id,
        Guid openAppId,
        Guid vehicleId,
        Guid vehicleDeviceId,
        string vehicleVin,
        string deviceVin,
        string vendorDeviceId,
        DateTime authorizationStartTime,
        DateTime? authorizationEndTime)
        : base(id)
    {
        OpenAppId = openAppId;
        VehicleId = vehicleId;
        VehicleDeviceId = vehicleDeviceId;
        VehicleVin = Check.NotNullOrWhiteSpace(vehicleVin, nameof(vehicleVin), VehicleConsts.MaxVinLength);
        DeviceVin = Check.NotNullOrWhiteSpace(deviceVin, nameof(deviceVin), VehicleConsts.MaxVinLength);
        VendorDeviceId = Check.NotNullOrWhiteSpace(vendorDeviceId, nameof(vendorDeviceId), VehicleDeviceConsts.MaxVendorDeviceIdLength);
        SetAuthorizationPeriod(authorizationStartTime, authorizationEndTime);
    }

    public void Renew(
        Guid vehicleDeviceId,
        string vehicleVin,
        string deviceVin,
        string vendorDeviceId,
        DateTime authorizationStartTime,
        DateTime? authorizationEndTime)
    {
        VehicleDeviceId = vehicleDeviceId;
        VehicleVin = Check.NotNullOrWhiteSpace(vehicleVin, nameof(vehicleVin), VehicleConsts.MaxVinLength);
        DeviceVin = Check.NotNullOrWhiteSpace(deviceVin, nameof(deviceVin), VehicleConsts.MaxVinLength);
        VendorDeviceId = Check.NotNullOrWhiteSpace(vendorDeviceId, nameof(vendorDeviceId), VehicleDeviceConsts.MaxVendorDeviceIdLength);
        SetAuthorizationPeriod(authorizationStartTime, authorizationEndTime);
    }

    public void Cancel(DateTime cancelTime)
    {
        if (cancelTime < AuthorizationStartTime)
        {
            throw new BusinessException("VNext:OpenPlatform:InvalidVehicleControlAuthorizationCancelTime")
                .WithData(nameof(cancelTime), cancelTime)
                .WithData(nameof(AuthorizationStartTime), AuthorizationStartTime);
        }

        AuthorizationEndTime = cancelTime;
    }

    private void SetAuthorizationPeriod(DateTime authorizationStartTime, DateTime? authorizationEndTime)
    {
        if (authorizationEndTime.HasValue && authorizationStartTime > authorizationEndTime.Value)
        {
            throw new BusinessException("VNext:OpenPlatform:InvalidVehicleControlAuthorizationPeriod")
                .WithData(nameof(authorizationStartTime), authorizationStartTime)
                .WithData(nameof(authorizationEndTime), authorizationEndTime);
        }

        AuthorizationStartTime = authorizationStartTime;
        AuthorizationEndTime = authorizationEndTime;
    }
}
