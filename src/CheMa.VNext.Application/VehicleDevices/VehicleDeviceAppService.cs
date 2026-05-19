using System;
using System.Linq;
using System.Threading.Tasks;
using CheMa.VNext.VehicleDevices.Models;

namespace CheMa.VNext.VehicleDevices;

public class VehicleDeviceAppService : VNextAppService, IVehicleDeviceAppService
{
    private readonly IVehicleDeviceService _vehicleDeviceService;

    public VehicleDeviceAppService(IVehicleDeviceService vehicleDeviceService)
    {
        _vehicleDeviceService = vehicleDeviceService;
    }

    public async Task<BindVehicleDeviceDto> BindAsync(BindVehicleDeviceInput input)
    {
        var result = await _vehicleDeviceService.BindAsync(new BindVehicleDeviceCommand
        {
            VehicleId = input.VehicleId,
            VendorType = input.VendorType,
            VendorDeviceId = input.VendorDeviceId
        });

        return new BindVehicleDeviceDto
        {
            VehicleId = result.VehicleId,
            VendorType = result.VendorType,
            VendorDeviceId = result.VendorDeviceId,
            Success = result.Success,
            AlreadyBound = result.AlreadyBound
        };
    }

    public async Task<UnbindVehicleDeviceDto> UnbindAsync(Guid vehicleId)
    {
        var result = await _vehicleDeviceService.UnbindAsync(new UnbindVehicleDeviceCommand
        {
            VehicleId = vehicleId
        });

        return new UnbindVehicleDeviceDto
        {
            VehicleId = result.VehicleId,
            VendorType = result.VendorType,
            VendorDeviceId = result.VendorDeviceId,
            Success = result.Success
        };
    }

    public async Task<VehicleDeviceLocationDto> GetLocationAsync(Guid vehicleId)
    {
        var result = await _vehicleDeviceService.GetLocationAsync(vehicleId);

        return new VehicleDeviceLocationDto
        {
            VehicleId = result.VehicleId,
            Longitude = result.Longitude,
            Latitude = result.Latitude,
            CoordinateSystem = result.CoordinateSystem,
            Speed = result.Speed,
            Direction = result.Direction,
            Address = result.Address,
            LocatedAtUtc = result.LocatedAtUtc,
            VendorType = result.VendorType,
            VendorDeviceId = result.VendorDeviceId
        };
    }

    public async Task<VehicleDeviceTrackDto> GetTrackAsync(GetVehicleDeviceTrackInput input)
    {
        var result = await _vehicleDeviceService.GetTrackAsync(new VehicleDeviceTrackQuery
        {
            VehicleId = input.VehicleId,
            StartTimeUtc = input.StartTimeUtc,
            EndTimeUtc = input.EndTimeUtc
        });

        return new VehicleDeviceTrackDto
        {
            VehicleId = result.VehicleId,
            CoordinateSystem = result.CoordinateSystem,
            Points = result.Points.Select(x => new VehicleDeviceTrackPointDto
            {
                Longitude = x.Longitude,
                Latitude = x.Latitude,
                Speed = x.Speed,
                Direction = x.Direction,
                Mileage = x.Mileage,
                LocatedAtUtc = x.LocatedAtUtc
            }).ToList()
        };
    }

    public async Task<VehicleDeviceStatusDto> GetStatusAsync(Guid vehicleId)
    {
        var result = await _vehicleDeviceService.GetStatusAsync(vehicleId);

        return new VehicleDeviceStatusDto
        {
            VehicleId = result.VehicleId,
            VendorType = result.VendorType,
            VendorDeviceId = result.VendorDeviceId,
            StatusTimeUtc = result.StatusTimeUtc,
            Basic = new VehicleDeviceBasicStatusDto
            {
                Online = result.Basic.Online,
                AccOn = result.Basic.AccOn,
                EngineOn = result.Basic.EngineOn,
                Speed = result.Basic.Speed,
                Mileage = result.Basic.Mileage,
                FuelLevelPercent = result.Basic.FuelLevelPercent,
                BatteryLevelPercent = result.Basic.BatteryLevelPercent,
                BatteryVoltage = result.Basic.BatteryVoltage
            },
            Body = new VehicleDeviceBodyStatusDto
            {
                Locked = result.Body.Locked,
                LeftFrontDoorOpen = result.Body.LeftFrontDoorOpen,
                RightFrontDoorOpen = result.Body.RightFrontDoorOpen,
                LeftRearDoorOpen = result.Body.LeftRearDoorOpen,
                RightRearDoorOpen = result.Body.RightRearDoorOpen,
                TrunkOpen = result.Body.TrunkOpen,
                HoodOpen = result.Body.HoodOpen,
                WindowOpen = result.Body.WindowOpen
            },
            Alert = new VehicleDeviceAlertStatusDto
            {
                HasAlert = result.Alert.HasAlert,
                Alerts = result.Alert.Alerts.Select(x => new VehicleDeviceAlertItemDto
                {
                    Code = x.Code,
                    Message = x.Message,
                    Level = x.Level,
                    AlertTimeUtc = x.AlertTimeUtc
                }).ToList()
            }
        };
    }

    public async Task<VehicleDeviceControlDto> ControlAsync(VehicleDeviceControlInput input)
    {
        var result = await _vehicleDeviceService.ControlAsync(new VehicleDeviceControlCommand
        {
            VehicleId = input.VehicleId,
            Action = input.Action
        });

        return new VehicleDeviceControlDto
        {
            VehicleId = result.VehicleId,
            Action = result.Action,
            Success = result.Success,
            Message = result.Message,
            VendorType = result.VendorType,
            VendorDeviceId = result.VendorDeviceId
        };
    }
}
