using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace CheMa.VNext.Vehicles.Entities;

public class Vehicle : FullAuditedAggregateRoot<Guid>
{
    public string Vin { get; private set; } = string.Empty;

    public string? PlateNumber { get; private set; }

    public string? Brand { get; private set; }

    public string? Series { get; private set; }

    public string? Model { get; private set; }

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
        string? brand,
        string? series,
        string? model,
        VehicleDeviceVendorType? vendorType,
        VehicleBindingStatus bindingStatus,
        DateTime? bindingTime)
        : base(id)
    {
        SetVin(vin);
        SetPlateNumber(plateNumber);
        SetVehicleProfile(brand, series, model);
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

    public void SetVehicleProfile(string? brand, string? series, string? model)
    {
        Brand = brand.IsNullOrWhiteSpace()
            ? null
            : Check.Length(brand, nameof(brand), VehicleConsts.MaxBrandLength, 0);
        Series = series.IsNullOrWhiteSpace()
            ? null
            : Check.Length(series, nameof(series), VehicleConsts.MaxSeriesLength, 0);
        Model = model.IsNullOrWhiteSpace()
            ? null
            : Check.Length(model, nameof(model), VehicleConsts.MaxModelLength, 0);
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
