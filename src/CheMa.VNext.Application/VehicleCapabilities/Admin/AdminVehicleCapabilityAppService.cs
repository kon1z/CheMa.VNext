using System;
using System.Linq;
using System.Threading.Tasks;
using CheMa.VNext.Base;
using CheMa.VNext.VehicleCapabilities.Shared;
using CheMa.VNext.VehicleDevices.Models;
using Volo.Abp;

namespace CheMa.VNext.VehicleCapabilities.Admin;

public class AdminVehicleCapabilityAppService : VNextAppService, IAdminVehicleCapabilityAppService
{
    private readonly AdminVehicleAccessContextFactory _accessContextFactory;
    private readonly IVehicleCapabilityOrchestrator _vehicleCapabilityOrchestrator;

    public AdminVehicleCapabilityAppService(
        AdminVehicleAccessContextFactory accessContextFactory,
        IVehicleCapabilityOrchestrator vehicleCapabilityOrchestrator)
    {
        _accessContextFactory = accessContextFactory;
        _vehicleCapabilityOrchestrator = vehicleCapabilityOrchestrator;
    }

    public async Task<VehicleCapabilityResultDto> GetVehicleInfoAsync(Guid vehicleId)
    {
        var context = await _accessContextFactory.CreateAsync();
        return await _vehicleCapabilityOrchestrator.GetVehicleInfoAsync(context, vehicleId);
    }

    public async Task<VehicleCapabilityResultDto> GetVehicleStatusAsync(Guid vehicleId)
    {
        var context = await _accessContextFactory.CreateAsync();
        return await _vehicleCapabilityOrchestrator.GetVehicleStatusAsync(context, vehicleId);
    }

    public async Task<VehicleCapabilityResultDto> GetVehicleLocationAsync(Guid vehicleId)
    {
        var context = await _accessContextFactory.CreateAsync();
        return await _vehicleCapabilityOrchestrator.GetVehicleLocationAsync(context, vehicleId);
    }

    public async Task<AdminVehicleTripListDto> GetVehicleTripsAsync(AdminVehicleTripQueryDto input)
    {
        Check.NotNull(input, nameof(input));

        var context = await _accessContextFactory.CreateAsync();
        var result = await _vehicleCapabilityOrchestrator.GetVehicleTripsAsync(
            context,
            new VehicleDeviceTripQuery
            {
                VehicleId = input.VehicleId,
                StartTimeUtc = input.StartTimeUtc,
                EndTimeUtc = input.EndTimeUtc
            });

        var trips = result.Trips
            .Where(x => !string.IsNullOrWhiteSpace(x.TripId))
            .ToList();
        var pageIndex = input.PageIndex <= 0 ? 1 : input.PageIndex;
        var pageSize = input.PageSize <= 0 ? 20 : input.PageSize;

        return new AdminVehicleTripListDto
        {
            TotalCount = trips.Count,
            Items = trips
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new AdminVehicleTripDto
                {
                    TripId = x.TripId,
                    StartTimeUtc = x.StartTimeUtc,
                    EndTimeUtc = x.EndTimeUtc,
                    StartAddress = x.StartAddress,
                    EndAddress = x.EndAddress,
                    Mileage = x.Mileage,
                    DurationSeconds = x.DurationSeconds,
                    AverageSpeed = x.AverageSpeed,
                    MaxSpeed = x.MaxSpeed
                })
                .ToList()
        };
    }

    public async Task<AdminVehicleTrackDto> GetVehicleTrackAsync(AdminVehicleTrackQueryDto input)
    {
        Check.NotNull(input, nameof(input));

        var context = await _accessContextFactory.CreateAsync();
        var result = await _vehicleCapabilityOrchestrator.GetVehicleTrackAsync(
            context,
            new VehicleDeviceTrackQuery
            {
                VehicleId = input.VehicleId,
                TripId = input.TripId,
                StartTimeUtc = input.StartTimeUtc,
                EndTimeUtc = input.EndTimeUtc
            });

        return new AdminVehicleTrackDto
        {
            VehicleId = input.VehicleId,
            TripId = input.TripId,
            Points = result.Points.Select(x => new AdminVehicleTrackPointDto
            {
                Longitude = x.Longitude,
                Latitude = x.Latitude,
                Speed = x.Speed,
                Direction = x.Direction,
                LocatedAtUtc = x.LocatedAtUtc
            }).ToList()
        };
    }

    public async Task<AdminVehicleAlertListDto> GetVehicleAlertsAsync(AdminVehicleAlertQueryDto input)
    {
        Check.NotNull(input, nameof(input));

        var context = await _accessContextFactory.CreateAsync();
        var result = await _vehicleCapabilityOrchestrator.GetVehicleAlertsAsync(
            context,
            new VehicleDeviceAlertQuery
            {
                VehicleId = input.VehicleId,
                StartTimeUtc = input.StartTimeUtc,
                EndTimeUtc = input.EndTimeUtc
            });

        var alerts = result.Alerts.Select(x => new AdminVehicleAlertDto
        {
            Code = x.Code,
            Message = x.Message,
            Level = x.Level.ToString(),
            AlertTimeUtc = x.AlertTimeUtc
        }).ToList();

        return new AdminVehicleAlertListDto
        {
            TotalCount = alerts.Count,
            Items = alerts
        };
    }
}
