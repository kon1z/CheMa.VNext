using System;
using CheMa.VNext.Vehicles.Enums;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace CheMa.VNext.Vehicles.Entities;

public class Vehicle : FullAuditedAggregateRoot<Guid>
{
    public string Vin { get; private set; } = string.Empty;

    public string? PlateNumber { get; private set; }

    public string EngineNumber { get; private set; } = string.Empty;

    public VehicleDeviceVendorType? VendorType { get; private set; }

    public VehicleBindingStatus BindingStatus { get; private set; }

    public DateTime? BindingTime { get; private set; }

    protected Vehicle()
    {
    }

    public Vehicle(
        Guid id,
        string vin,
        string? plateNumber,
        string engineNumber,
        VehicleDeviceVendorType? vendorType,
        VehicleBindingStatus bindingStatus,
        DateTime? bindingTime)
        : base(id)
    {
        SetVin(vin);
        SetPlateNumber(plateNumber);
        SetEngineNumber(engineNumber);
        SetBindingInfo(vendorType, bindingStatus, bindingTime);
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

    public void SetEngineNumber(string engineNumber)
    {
        EngineNumber = Check.NotNullOrWhiteSpace(engineNumber, nameof(engineNumber), VehicleConsts.MaxEngineNumberLength);
    }

    public void SetBindingInfo(
        VehicleDeviceVendorType? vendorType,
        VehicleBindingStatus bindingStatus,
        DateTime? bindingTime)
    {
        VendorType = bindingStatus == VehicleBindingStatus.Bound
            ? Check.NotNull(vendorType, nameof(vendorType))
            : null;
        BindingStatus = bindingStatus;
        BindingTime = bindingStatus == VehicleBindingStatus.Bound
            ? Check.NotNull(bindingTime, nameof(bindingTime))
            : null;
    }
}
