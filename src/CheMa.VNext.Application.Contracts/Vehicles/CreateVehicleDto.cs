using System;
using System.ComponentModel.DataAnnotations;

namespace CheMa.VNext.Vehicles;

public class CreateVehicleDto
{
    [Required]
    [StringLength(VehicleConsts.MaxVinLength)]
    public string Vin { get; set; } = default!;

    [StringLength(VehicleConsts.MaxPlateNumberLength)]
    public string? PlateNumber { get; set; }

    [Required]
    public VehicleDeviceType DeviceType { get; set; }

    [Required]
    public VehicleBindingStatus BindingStatus { get; set; }

    public DateTime? BindingTime { get; set; }
}
