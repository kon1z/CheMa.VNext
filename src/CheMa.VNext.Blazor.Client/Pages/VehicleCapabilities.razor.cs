using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheMa.VNext.Permissions;
using CheMa.VNext.VehicleCapabilities.Admin;
using CheMa.VNext.VehicleCapabilities.Shared;
using CheMa.VNext.Vehicles.Dtos;
using CheMa.VNext.Vehicles.Inputs;
using Microsoft.AspNetCore.Components;

namespace CheMa.VNext.Blazor.Client.Pages;

public partial class VehicleCapabilities
{
    private const string PolicyName = VehicleCapabilityPermissions.View;

    protected List<VehicleDto> Vehicles { get; set; } = [];

    protected string? SelectedVehicleIdText { get; set; }

    protected DateTime StartTime { get; set; } = DateTime.Now.AddDays(-1);

    protected DateTime EndTime { get; set; } = DateTime.Now;

    protected bool IsBusy { get; set; }

    protected VehicleCapabilityResultDto? CurrentInfo { get; set; }

    protected List<AdminVehicleTripDto> Trips { get; set; } = [];

    protected int TripsTotalCount { get; set; }

    protected List<AdminVehicleTrackPointDto> TrackPoints { get; set; } = [];

    protected string? CurrentTripId { get; set; }

    protected List<AdminVehicleAlertDto> VehicleAlerts { get; set; } = [];

    protected int AlertsTotalCount { get; set; }

    protected string SelectedVehicleSummary => SelectedVehicle == null ? L["VehicleCapabilities:NoVehicleSelected"] : $"{SelectedVehicle.Vin} {FormatPlate(SelectedVehicle)}";

    protected string TrackSummary => string.IsNullOrWhiteSpace(CurrentTripId)
        ? L["VehicleCapabilities:TrackHelp"]
        : $"{L["VehicleCapabilities:TripId"]}: {CurrentTripId}";

    private VehicleDto? SelectedVehicle => Guid.TryParse(SelectedVehicleIdText, out var vehicleId)
        ? Vehicles.FirstOrDefault(x => x.Id == vehicleId)
        : null;

    protected override async Task OnInitializedAsync()
    {
        await LoadVehiclesAsync();
    }

    protected async Task LoadVehicleInfoAsync()
    {
        var vehicleId = await RequireVehicleIdAsync();
        if (!vehicleId.HasValue)
        {
            return;
        }

        await RunAsync(async () =>
        {
            CurrentInfo = await AdminVehicleCapabilityAppService.GetVehicleInfoAsync(vehicleId.Value);
        });
    }

    protected async Task LoadTripsAsync()
    {
        var vehicleId = await RequireVehicleIdAsync();
        if (!vehicleId.HasValue)
        {
            return;
        }

        await RunAsync(async () =>
        {
            var result = await AdminVehicleCapabilityAppService.GetVehicleTripsAsync(new AdminVehicleTripQueryDto
            {
                VehicleId = vehicleId.Value,
                StartTimeUtc = StartTime.ToUniversalTime(),
                EndTimeUtc = EndTime.ToUniversalTime(),
                PageIndex = 1,
                PageSize = 20
            });

            Trips = result.Items.ToList();
            TripsTotalCount = result.TotalCount;
        });
    }

    protected async Task LoadTrackAsync(string? tripId)
    {
        var vehicleId = await RequireVehicleIdAsync();
        if (!vehicleId.HasValue)
        {
            return;
        }

        await RunAsync(async () =>
        {
            CurrentTripId = tripId;
            var result = await AdminVehicleCapabilityAppService.GetVehicleTrackAsync(new AdminVehicleTrackQueryDto
            {
                VehicleId = vehicleId.Value,
                TripId = tripId,
                StartTimeUtc = StartTime.ToUniversalTime(),
                EndTimeUtc = EndTime.ToUniversalTime()
            });

            TrackPoints = result.Points.ToList();
        });
    }

    protected async Task LoadAlertsAsync()
    {
        var vehicleId = await RequireVehicleIdAsync();
        if (!vehicleId.HasValue)
        {
            return;
        }

        await RunAsync(async () =>
        {
            var result = await AdminVehicleCapabilityAppService.GetVehicleAlertsAsync(new AdminVehicleAlertQueryDto
            {
                VehicleId = vehicleId.Value,
                StartTimeUtc = StartTime.ToUniversalTime(),
                EndTimeUtc = EndTime.ToUniversalTime()
            });

            VehicleAlerts = result.Items.ToList();
            AlertsTotalCount = result.TotalCount;
        });
    }

    protected async Task LoadAllAsync()
    {
        await LoadVehicleInfoAsync();
        await LoadTripsAsync();
        await LoadAlertsAsync();
    }

    protected void OnVehicleChanged(ChangeEventArgs args)
    {
        SelectedVehicleIdText = args.Value?.ToString();
        CurrentInfo = null;
        Trips = [];
        TripsTotalCount = 0;
        TrackPoints = [];
        CurrentTripId = null;
        VehicleAlerts = [];
        AlertsTotalCount = 0;
    }

    protected void OnStartTimeChanged(ChangeEventArgs args)
    {
        StartTime = ParseDateTimeLocal(args.Value?.ToString()) ?? StartTime;
    }

    protected void OnEndTimeChanged(ChangeEventArgs args)
    {
        EndTime = ParseDateTimeLocal(args.Value?.ToString()) ?? EndTime;
    }

    protected string FormatPlate(VehicleDto vehicle)
    {
        return string.IsNullOrWhiteSpace(vehicle.PlateNumber) ? string.Empty : $"/ {vehicle.PlateNumber}";
    }

    protected string FormatDateTime(DateTime? value)
    {
        return value?.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") ?? "-";
    }

    protected string FormatBool(bool? value)
    {
        return value.HasValue ? (value.Value ? L["VehicleCapabilities:Yes"] : L["VehicleCapabilities:No"]) : "-";
    }

    protected string FormatDecimal(decimal? value)
    {
        return value?.ToString("0.##") ?? "-";
    }

    protected string ToDateTimeLocalValue(DateTime? value)
    {
        return value?.ToLocalTime().ToString("yyyy-MM-ddTHH:mm") ?? string.Empty;
    }

    private async Task LoadVehiclesAsync()
    {
        var result = await VehicleAppService.GetListAsync(new GetVehicleListInput
        {
            MaxResultCount = 100,
            SkipCount = 0,
            Sorting = nameof(VehicleDto.CreationTime) + " desc"
        });

        Vehicles = result.Items.ToList();
    }

    private async Task<Guid?> RequireVehicleIdAsync()
    {
        if (!Guid.TryParse(SelectedVehicleIdText, out var vehicleId))
        {
            await UiMessageService.Warn(L["VehicleCapabilities:SelectVehicle"]);
            return null;
        }

        return vehicleId;
    }

    private async Task RunAsync(Func<Task> action)
    {
        try
        {
            IsBusy = true;
            await action();
        }
        catch (Exception ex)
        {
            await UiMessageService.Error(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static DateTime? ParseDateTimeLocal(string? value)
    {
        return DateTime.TryParse(value, out var result) ? result : null;
    }
}
