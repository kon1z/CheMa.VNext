using System;
using CheMa.VNext.Vehicles.Enums;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace CheMa.VNext.VehicleDevices.Entities;

public class VehicleDevice : FullAuditedAggregateRoot<Guid>
{
    public Guid? VehicleId { get; private set; }

    public VehicleDeviceVendorType VendorType { get; private set; }

    public string VendorDeviceId { get; private set; } = string.Empty;

    protected VehicleDevice()
    {
    }

    public VehicleDevice(
        Guid id,
        VehicleDeviceVendorType vendorType,
        string vendorDeviceId,
        Guid? vehicleId = null)
        : base(id)
    {
        VendorType = vendorType;
        VendorDeviceId = Check.NotNullOrWhiteSpace(vendorDeviceId, nameof(vendorDeviceId), VehicleDeviceConsts.MaxVendorDeviceIdLength);
        VehicleId = vehicleId;
    }

    public void Bind(Guid vehicleId)
    {
        VehicleId = vehicleId;
    }

    public void Unbind()
    {
        VehicleId = null;
    }
}
