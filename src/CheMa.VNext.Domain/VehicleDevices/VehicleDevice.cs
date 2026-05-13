using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace CheMa.VNext.VehicleDevices;

public class VehicleDevice : FullAuditedAggregateRoot<Guid>
{
    public Guid VehicleId { get; private set; }

    public string Brand { get; private set; } = string.Empty;

    public string VendorDeviceId { get; private set; } = string.Empty;

    public string Vin { get; private set; } = string.Empty;

    public VehicleDeviceStatus Status { get; private set; }

    public DateTime? BoundTime { get; private set; }

    public DateTime? UnboundTime { get; private set; }

    protected VehicleDevice()
    {
    }

    public VehicleDevice(
        Guid id,
        Guid vehicleId,
        string brand,
        string vendorDeviceId,
        string vin)
        : base(id)
    {
        VehicleId = vehicleId;
        Brand = Check.NotNullOrWhiteSpace(brand, nameof(brand), VehicleDeviceConsts.MaxBrandLength);
        VendorDeviceId = Check.NotNullOrWhiteSpace(vendorDeviceId, nameof(vendorDeviceId), VehicleDeviceConsts.MaxVendorDeviceIdLength);
        Vin = Check.NotNullOrWhiteSpace(vin, nameof(vin), VehicleDeviceConsts.MaxVinLength);
        Status = VehicleDeviceStatus.Bound;
        BoundTime = DateTime.UtcNow;
    }

    public void Unbind()
    {
        if (Status == VehicleDeviceStatus.Unbound)
        {
            return;
        }

        Status = VehicleDeviceStatus.Unbound;
        UnboundTime = DateTime.UtcNow;
    }
}
