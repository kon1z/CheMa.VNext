using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace CheMa.VNext.Vehicles;

public class Vehicle : FullAuditedAggregateRoot<Guid>
{
    public string Vin { get; private set; } = string.Empty;

    public string? PlateNumber { get; private set; }

    public VehicleDeviceType DeviceType { get; private set; }

    public VehicleBindingStatus BindingStatus { get; private set; }

    public DateTime? BindingTime { get; private set; }

    protected Vehicle()
    {
    }

    public Vehicle(
        Guid id,
        string vin,
        string? plateNumber,
        VehicleDeviceType deviceType,
        VehicleBindingStatus bindingStatus,
        DateTime? bindingTime)
        : base(id)
    {
        SetVin(vin);
        SetPlateNumber(plateNumber);
        SetBindingInfo(deviceType, bindingStatus, bindingTime);
    }

    public void SetVin(string vin)
    {
        Vin = Check.NotNullOrWhiteSpace(vin, nameof(vin), VehicleConsts.MaxVinLength);
    }

    public void SetPlateNumber(string? plateNumber)
    {
        PlateNumber = plateNumber.IsNullOrWhiteSpace()
            ? null
            : Check.Length(plateNumber, nameof(plateNumber), VehicleConsts.MaxPlateNumberLength, 0);
    }

    public void SetBindingInfo(
        VehicleDeviceType deviceType,
        VehicleBindingStatus bindingStatus,
        DateTime? bindingTime)
    {
        DeviceType = deviceType;
        BindingStatus = bindingStatus;
        BindingTime = bindingStatus == VehicleBindingStatus.Bound
            ? Check.NotNull(bindingTime, nameof(bindingTime))
            : bindingTime;
    }
}
