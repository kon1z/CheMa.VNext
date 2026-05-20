using System;
using System.ComponentModel.DataAnnotations;
using CheMa.VNext.Vehicles.Enums;

namespace CheMa.VNext.Vehicles.Dtos;

public class CreateVehicleDto
{
    [Required]
    [StringLength(VehicleConsts.MaxVinLength)]
    public string Vin { get; set; } = default!;

    [StringLength(VehicleConsts.MaxPlateNumberLength)]
    public string? PlateNumber { get; set; }

    [Required]
    [StringLength(VehicleConsts.MaxEngineNumberLength)]
    public string EngineNumber { get; set; } = default!;

    public VehicleDeviceVendorType? VendorType { get; set; }

    [Required]
    public VehicleBindingStatus BindingStatus { get; set; }

    public DateTime? BindingTime { get; set; }
}
