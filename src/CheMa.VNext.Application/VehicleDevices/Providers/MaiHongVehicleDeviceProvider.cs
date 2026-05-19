using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CheMa.VNext.MaiHong;
using CheMa.VNext.VehicleDevices.Models;
using CheMa.VNext.Vehicles;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace CheMa.VNext.VehicleDevices.Providers;

public class MaiHongVehicleDeviceProvider : IVehicleDeviceProvider, ITransientDependency
{
    public VehicleDeviceVendorType VendorType => VehicleDeviceVendorType.MaiHong;

    private readonly IMaiHongGateway _maiHongGateway;

    public MaiHongVehicleDeviceProvider(IMaiHongGateway maiHongGateway)
    {
        _maiHongGateway = maiHongGateway;
    }

    public bool SupportsControlAction(VehicleDeviceControlAction action)
    {
        return action is VehicleDeviceControlAction.Lock
            or VehicleDeviceControlAction.Unlock
            or VehicleDeviceControlAction.FindCar;
    }

    public async Task BindAsync(VehicleDeviceBindingContext context, CancellationToken cancellationToken = default)
    {
        Check.NotNull(context, nameof(context));

        var response = await _maiHongGateway.AddVehicleAsync(new MaiHongVehicleCreateRequest
        {
            Vin = context.Vin,
            EquipmentCode = context.VendorDeviceId
        }, cancellationToken);

        EnsureSuccess(response, "MaiHong bind vehicle failed.", context.VendorDeviceId);
    }

    public async Task UnbindAsync(VehicleDeviceBindingContext context, CancellationToken cancellationToken = default)
    {
        Check.NotNull(context, nameof(context));

        var response = await _maiHongGateway.SetServiceSwitchByEquipmentCodeAsync(
            context.VendorDeviceId,
            0,
            cancellationToken);

        EnsureSuccess(response, "MaiHong unbind vehicle failed.", context.VendorDeviceId);
    }

    public async Task<VehicleDeviceLocationResult> GetLocationAsync(VehicleDeviceContext context, CancellationToken cancellationToken = default)
    {
        Check.NotNull(context, nameof(context));

        var response = await _maiHongGateway.GetPositionsAsync(context.VendorDeviceId, cancellationToken);
        EnsureSuccess(response, "MaiHong get location failed.", context.VendorDeviceId);

        var position = response.Data?.Positions?.FirstOrDefault()
            ?? throw CreateVendorException("MaiHong location not found.", context.VendorDeviceId);

        return new VehicleDeviceLocationResult
        {
            VehicleId = context.VehicleId,
            VendorType = VehicleDeviceVendorType.MaiHong,
            VendorDeviceId = context.VendorDeviceId,
            Longitude = ParseDecimal(position.Lon, "Longitude"),
            Latitude = ParseDecimal(position.Lat, "Latitude"),
            Speed = TryParseDecimal(position.ExtensionData, "speed"),
            Direction = TryParseDecimal(position.ExtensionData, "direction"),
            LocatedAtUtc = ParseDateTime(position.ReportTime),
            CoordinateSystem = VehicleDeviceConsts.CoordinateSystemBd09
        };
    }

    public async Task<VehicleDeviceTrackResult> GetTrackAsync(VehicleDeviceContext context, VehicleDeviceTrackQuery query, CancellationToken cancellationToken = default)
    {
        Check.NotNull(context, nameof(context));
        Check.NotNull(query, nameof(query));

        var response = await _maiHongGateway.GetTracesAsync(new MaiHongTracesQuery
        {
            VehicleHwid = context.VendorDeviceId,
            DateFrom = query.StartTimeUtc.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            DateTo = query.EndTimeUtc.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            Type = 1
        }, cancellationToken);

        EnsureSuccess(response, "MaiHong get track failed.", context.VendorDeviceId);

        return new VehicleDeviceTrackResult
        {
            VehicleId = context.VehicleId,
            CoordinateSystem = VehicleDeviceConsts.CoordinateSystemBd09,
            Points = response.Data?.Trace?.Select(x => new VehicleDeviceTrackPoint
            {
                Longitude = ParseDecimal(x.Lon, "Longitude"),
                Latitude = ParseDecimal(x.Lat, "Latitude"),
                LocatedAtUtc = ParseDateTime(x.Time)
            }).ToList() ?? []
        };
    }

    public async Task<VehicleDeviceStatusResult> GetStatusAsync(VehicleDeviceContext context, CancellationToken cancellationToken = default)
    {
        Check.NotNull(context, nameof(context));

        var response = await _maiHongGateway.GetVehicleStatusAsync(context.VendorDeviceId, cancellationToken);
        EnsureSuccess(response, "MaiHong get status failed.", context.VendorDeviceId);

        var status = response.Data?.FirstOrDefault()
            ?? throw CreateVendorException("MaiHong status not found.", context.VendorDeviceId);

        return new VehicleDeviceStatusResult
        {
            VehicleId = context.VehicleId,
            VendorType = VehicleDeviceVendorType.MaiHong,
            VendorDeviceId = context.VendorDeviceId,
            StatusTimeUtc = ParseDateTime(status.ReportTime),
            Basic = new VehicleDeviceBasicStatus
            {
                Online = TryParseBoolean(status.ExtensionData, "onlineState"),
                AccOn = TryParseBoolean(status.ExtensionData, "accStatus") ?? ParseBoolean(status.Engine),
                EngineOn = ParseBoolean(status.Engine),
                Speed = TryParseDecimal(status.ExtensionData, "speed"),
                Mileage = TryParseDecimal(status.ExtensionData, "originalMileage") ?? TryParseDecimal(status.ExtensionData, "mileage"),
                FuelLevelPercent = TryParseDecimal(status.ExtensionData, "fuelLevel"),
                BatteryLevelPercent = TryParseDecimal(status.ExtensionData, "soc"),
                BatteryVoltage = TryParseDecimal(status.ExtensionData, "carVoltage") ?? TryParseDecimal(status.ExtensionData, "voltage")
            },
            Body = new VehicleDeviceBodyStatus
            {
                Locked = ParseBoolean(status.Lock),
                LeftFrontDoorOpen = TryParseBoolean(status.ExtensionData, "door1Status"),
                RightFrontDoorOpen = TryParseBoolean(status.ExtensionData, "door2Status"),
                LeftRearDoorOpen = TryParseBoolean(status.ExtensionData, "door3Status"),
                RightRearDoorOpen = TryParseBoolean(status.ExtensionData, "door4Status"),
                TrunkOpen = TryParseBoolean(status.ExtensionData, "door5Status"),
                HoodOpen = TryParseBoolean(status.ExtensionData, "bonnet"),
                WindowOpen = TryParseBoolean(status.ExtensionData, "windowStatus"),
                DefendOn = ParseBoolean(status.IsInDefend)
            },
            Alert = new VehicleDeviceAlertStatus
            {
                HasAlert = false,
                Alerts = []
            }
        };
    }

    public async Task<VehicleDeviceControlResult> ControlAsync(VehicleDeviceContext context, VehicleDeviceControlAction action, CancellationToken cancellationToken = default)
    {
        Check.NotNull(context, nameof(context));

        var order = action switch
        {
            VehicleDeviceControlAction.Lock => 20,
            VehicleDeviceControlAction.Unlock => 21,
            VehicleDeviceControlAction.FindCar => 5,
            _ => throw new BusinessException(VehicleDeviceErrorCodes.CapabilityNotSupported)
                .WithData("Action", action)
        };

        var response = await _maiHongGateway.ControlVehicleAsync(new MaiHongVehicleControlRequest
        {
            VehicleHwid = context.VendorDeviceId,
            Order = { ["order"] = order }
        }, cancellationToken);

        EnsureSuccess(response, "MaiHong control failed.", context.VendorDeviceId);

        return new VehicleDeviceControlResult
        {
            VehicleId = context.VehicleId,
            Action = action,
            Success = true,
            VendorType = VehicleDeviceVendorType.MaiHong,
            VendorDeviceId = context.VendorDeviceId,
            Message = "OK"
        };
    }

    private static void EnsureSuccess(MaiHongResponse response, string message, string vendorDeviceId)
    {
        if (response.Errno.HasValue && response.Errno.Value.ValueKind != System.Text.Json.JsonValueKind.Null)
        {
            var errno = response.Errno.Value;
            if (errno.ValueKind == System.Text.Json.JsonValueKind.Number && errno.TryGetInt32(out var number) && number == 0)
            {
                return;
            }

            if (errno.ValueKind == System.Text.Json.JsonValueKind.String && string.Equals(errno.GetString(), "0", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        if (string.IsNullOrWhiteSpace(response.Error))
        {
            return;
        }

        throw CreateVendorException(message, vendorDeviceId, response.Error);
    }

    private static BusinessException CreateVendorException(string message, string vendorDeviceId, string? error = null)
    {
        return new BusinessException(VehicleDeviceErrorCodes.VendorRequestFailed)
            .WithData("Message", message)
            .WithData("VendorDeviceId", vendorDeviceId)
            .WithData("Error", error ?? string.Empty);
    }

    private static decimal ParseDecimal(string? value, string fieldName)
    {
        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        throw new BusinessException(VehicleDeviceErrorCodes.VendorRequestFailed)
            .WithData("Field", fieldName)
            .WithData("Value", value ?? string.Empty);
    }

    private static DateTime ParseDateTime(string? value)
    {
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var result))
        {
            return result.ToUniversalTime();
        }

        return DateTime.UtcNow;
    }

    private static bool? ParseBoolean(string? value)
    {
        return value switch
        {
            "1" => true,
            "0" => false,
            "true" => true,
            "false" => false,
            _ => null
        };
    }

    private static decimal? TryParseDecimal(System.Collections.Generic.Dictionary<string, System.Text.Json.JsonElement>? extensionData, string key)
    {
        if (extensionData == null || !extensionData.TryGetValue(key, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            System.Text.Json.JsonValueKind.Number when value.TryGetDecimal(out var number) => number,
            System.Text.Json.JsonValueKind.String when decimal.TryParse(value.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var number) => number,
            _ => null
        };
    }

    private static bool? TryParseBoolean(System.Collections.Generic.Dictionary<string, System.Text.Json.JsonElement>? extensionData, string key)
    {
        if (extensionData == null || !extensionData.TryGetValue(key, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            System.Text.Json.JsonValueKind.True => true,
            System.Text.Json.JsonValueKind.False => false,
            System.Text.Json.JsonValueKind.Number when value.TryGetInt32(out var number) => number != 0,
            System.Text.Json.JsonValueKind.String => ParseBoolean(value.GetString()),
            _ => null
        };
    }
}