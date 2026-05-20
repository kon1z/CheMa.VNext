using System;
using System.ComponentModel.DataAnnotations;

namespace CheMa.VNext.Vehicles.Dtos;

public class UpdateVehicleDto
{
    [Required]
    [StringLength(VehicleConsts.MaxVinLength)]
    public string Vin { get; set; } = default!;

    [StringLength(VehicleConsts.MaxPlateNumberLength)]
    public string? PlateNumber { get; set; }

    [StringLength(VehicleConsts.MaxBrandLength)]
    public string? Brand { get; set; }

    [StringLength(VehicleConsts.MaxSeriesLength)]
    public string? Series { get; set; }

    [StringLength(VehicleConsts.MaxModelLength)]
    public string? Model { get; set; }

    public VehicleDeviceVendorType? VendorType { get; set; }

    [Required]
    public VehicleBindingStatus BindingStatus { get; set; }

    public DateTime? BindingTime { get; set; }
}
