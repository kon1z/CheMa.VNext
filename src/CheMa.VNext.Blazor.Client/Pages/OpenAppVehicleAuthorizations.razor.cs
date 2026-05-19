using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheMa.VNext.OpenPlatform;
using CheMa.VNext.OpenPlatform.Dtos;
using CheMa.VNext.OpenPlatform.Inputs;
using CheMa.VNext.Permissions;
using CheMa.VNext.Vehicles;
using CheMa.VNext.Vehicles.Dtos;
using CheMa.VNext.Vehicles.Inputs;
using Microsoft.AspNetCore.Components;

namespace CheMa.VNext.Blazor.Client.Pages;

public partial class OpenAppVehicleAuthorizations
{
    private const string PolicyName = OpenPlatformPermissions.VehicleAuthorizations.Default;
    private const string ManagePolicyName = OpenPlatformPermissions.VehicleAuthorizations.Manage;

    [SupplyParameterFromQuery(Name = "openAppId")]
    public Guid? OpenAppId { get; set; }

    protected List<OpenAppDto> OpenApps { get; set; } = [];

    protected List<VehicleDto> Vehicles { get; set; } = [];

    protected List<OpenAppVehicleAuthorizationDto> AuthorizationItems { get; set; } = [];

    protected List<OpenAppVehicleAuthorizationDto> FilteredItems { get; set; } = [];

    protected string? SelectedVehicleIdText { get; set; }

    protected bool IsAuthorizeModalVisible { get; set; }

    protected bool IsRenewModalVisible { get; set; }

    protected bool IsCancelModalVisible { get; set; }

    protected bool IsDetailModalVisible { get; set; }

    protected AuthorizationCreateViewModel AuthorizeModel { get; set; } = AuthorizationCreateViewModel.CreateDefault();

    protected AuthorizationRenewViewModel RenewModel { get; set; } = AuthorizationRenewViewModel.CreateDefault();

    protected AuthorizationCancelViewModel CancelModel { get; set; } = AuthorizationCancelViewModel.CreateDefault();

    protected OpenAppVehicleAuthorizationDto? DetailItem { get; set; }

    protected OpenAppVehicleAuthorizationDto? RenewingItem { get; set; }

    protected OpenAppVehicleAuthorizationDto? CancellingItem { get; set; }

    protected OpenAppDto? CurrentOpenApp => OpenAppId.HasValue ? OpenApps.FirstOrDefault(x => x.Id == OpenAppId.Value) : null;

    protected string RenewingItemSummary => RenewingItem == null ? string.Empty : $"{GetOpenAppName(RenewingItem.OpenAppId)} / {RenewingItem.VehicleVin}";

    protected string CancellingItemSummary => CancellingItem == null ? string.Empty : $"{GetOpenAppName(CancellingItem.OpenAppId)} / {CancellingItem.VehicleVin}";

    protected override async Task OnInitializedAsync()
    {
        await LoadReferenceDataAsync();
        ApplyFilter();
    }

    protected Task LoadAsync()
    {
        ApplyFilter();
        return Task.CompletedTask;
    }

    protected Task OpenAuthorizeModalAsync()
    {
        AuthorizeModel = AuthorizationCreateViewModel.CreateDefault(OpenAppId);
        IsAuthorizeModalVisible = true;
        return Task.CompletedTask;
    }

    protected Task CloseAuthorizeModalAsync()
    {
        IsAuthorizeModalVisible = false;
        AuthorizeModel = AuthorizationCreateViewModel.CreateDefault(OpenAppId);
        return Task.CompletedTask;
    }

    protected Task OpenRenewModalAsync(OpenAppVehicleAuthorizationDto item)
    {
        RenewingItem = item;
        RenewModel = AuthorizationRenewViewModel.FromDto(item);
        IsRenewModalVisible = true;
        return Task.CompletedTask;
    }

    protected Task CloseRenewModalAsync()
    {
        IsRenewModalVisible = false;
        RenewingItem = null;
        RenewModel = AuthorizationRenewViewModel.CreateDefault();
        return Task.CompletedTask;
    }

    protected Task OpenCancelModalAsync(OpenAppVehicleAuthorizationDto item)
    {
        CancellingItem = item;
        CancelModel = AuthorizationCancelViewModel.CreateDefault();
        IsCancelModalVisible = true;
        return Task.CompletedTask;
    }

    protected Task CloseCancelModalAsync()
    {
        IsCancelModalVisible = false;
        CancellingItem = null;
        CancelModel = AuthorizationCancelViewModel.CreateDefault();
        return Task.CompletedTask;
    }

    protected async Task OpenDetailModalAsync(OpenAppVehicleAuthorizationDto item)
    {
        DetailItem = await OpenAppVehicleAuthorizationAppService.GetAsync(item.Id);
        IsDetailModalVisible = true;
    }

    protected Task CloseDetailModalAsync()
    {
        IsDetailModalVisible = false;
        DetailItem = null;
        return Task.CompletedTask;
    }

    protected async Task AuthorizeAsync()
    {
        if (!Guid.TryParse(AuthorizeModel.OpenAppIdText, out var openAppId))
        {
            await UiMessageService.Warn(L["OpenPlatform:SelectApp"]);
            return;
        }

        if (!Guid.TryParse(AuthorizeModel.VehicleIdText, out var vehicleId))
        {
            await UiMessageService.Warn(L["OpenPlatform:SelectVehicle"]);
            return;
        }

        try
        {
            var result = await OpenAppVehicleAuthorizationAppService.AuthorizeAsync(openAppId, new AuthorizeOpenAppVehicleDto
            {
                VehicleId = vehicleId,
                AuthorizationStartTime = AuthorizeModel.AuthorizationStartTime,
                AuthorizationEndTime = AuthorizeModel.AuthorizationEndTime
            });

            MergeItem(result);
            await UiMessageService.Success(L["OpenPlatform:OperationSucceeded"]);
            await CloseAuthorizeModalAsync();
        }
        catch (Exception ex)
        {
            await UiMessageService.Error(ex.Message);
        }
    }

    protected async Task RenewAsync()
    {
        if (RenewingItem == null)
        {
            return;
        }

        try
        {
            var result = await OpenAppVehicleAuthorizationAppService.RenewAsync(RenewingItem.Id, new RenewOpenAppVehicleAuthorizationDto
            {
                AuthorizationStartTime = RenewModel.AuthorizationStartTime,
                AuthorizationEndTime = RenewModel.AuthorizationEndTime
            });

            MergeItem(result);
            await UiMessageService.Success(L["OpenPlatform:OperationSucceeded"]);
            await CloseRenewModalAsync();
        }
        catch (Exception ex)
        {
            await UiMessageService.Error(ex.Message);
        }
    }

    protected async Task CancelAsync()
    {
        if (CancellingItem == null)
        {
            return;
        }

        try
        {
            await OpenAppVehicleAuthorizationAppService.CancelAsync(CancellingItem.Id, new CancelOpenAppVehicleAuthorizationDto
            {
                CancelTime = CancelModel.CancelTime
            });

            var updated = await OpenAppVehicleAuthorizationAppService.GetAsync(CancellingItem.Id);
            MergeItem(updated);
            await UiMessageService.Success(L["OpenPlatform:OperationSucceeded"]);
            await CloseCancelModalAsync();
        }
        catch (Exception ex)
        {
            await UiMessageService.Error(ex.Message);
        }
    }

    protected void OnVehicleChanged(ChangeEventArgs args)
    {
        SelectedVehicleIdText = args.Value?.ToString();
        ApplyFilter();
    }

    protected void OnAuthorizeOpenAppChanged(ChangeEventArgs args) => AuthorizeModel.OpenAppIdText = args.Value?.ToString();

    protected void OnAuthorizeVehicleChanged(ChangeEventArgs args) => AuthorizeModel.VehicleIdText = args.Value?.ToString();

    protected void OnAuthorizeStartTimeChanged(ChangeEventArgs args) => AuthorizeModel.AuthorizationStartTime = ParseDateTimeLocal(args.Value?.ToString()) ?? DateTime.Now;

    protected void OnAuthorizeEndTimeChanged(ChangeEventArgs args) => AuthorizeModel.AuthorizationEndTime = ParseDateTimeLocal(args.Value?.ToString()) ?? DateTime.Now.AddDays(30);

    protected void OnRenewStartTimeChanged(ChangeEventArgs args) => RenewModel.AuthorizationStartTime = ParseDateTimeLocal(args.Value?.ToString()) ?? DateTime.Now;

    protected void OnRenewEndTimeChanged(ChangeEventArgs args) => RenewModel.AuthorizationEndTime = ParseDateTimeLocal(args.Value?.ToString()) ?? DateTime.Now.AddDays(30);

    protected void OnCancelTimeChanged(ChangeEventArgs args) => CancelModel.CancelTime = ParseDateTimeLocal(args.Value?.ToString());

    protected string GetOpenAppName(Guid openAppId)
    {
        return OpenApps.FirstOrDefault(x => x.Id == openAppId)?.Name ?? openAppId.ToString();
    }

    protected string GetStatusText(OpenAppVehicleAuthorizationDto item)
    {
        var now = DateTime.Now;
        if (item.AuthorizationEndTime.HasValue && item.AuthorizationEndTime.Value < now)
        {
            return L["OpenPlatform:AuthorizationExpired"];
        }

        if (item.AuthorizationStartTime > now)
        {
            return L["OpenPlatform:AuthorizationPending"];
        }

        return L["OpenPlatform:AuthorizationActive"];
    }

    protected string GetStatusBadgeClass(OpenAppVehicleAuthorizationDto item)
    {
        var now = DateTime.Now;
        if (item.AuthorizationEndTime.HasValue && item.AuthorizationEndTime.Value < now)
        {
            return "text-bg-secondary";
        }

        if (item.AuthorizationStartTime > now)
        {
            return "text-bg-warning";
        }

        return "text-bg-success";
    }

    protected string FormatDateTime(DateTime? value)
    {
        return value?.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") ?? "-";
    }

    protected string ToDateTimeLocalValue(DateTime? value)
    {
        return value?.ToLocalTime().ToString("yyyy-MM-ddTHH:mm") ?? string.Empty;
    }

    private async Task LoadReferenceDataAsync()
    {
        var openAppResult = await OpenAppAppService.GetListAsync(new GetOpenAppListInput
        {
            MaxResultCount = 100,
            SkipCount = 0,
            Sorting = nameof(OpenAppDto.CreationTime) + " DESC"
        });

        OpenApps = openAppResult.Items.ToList();

        var vehicleResult = await VehicleAppService.GetListAsync(new GetVehicleListInput
        {
            MaxResultCount = 100,
            SkipCount = 0,
            Sorting = nameof(VehicleDto.CreationTime) + " desc"
        });

        Vehicles = vehicleResult.Items.ToList();
    }

    private void ApplyFilter()
    {
        IEnumerable<OpenAppVehicleAuthorizationDto> query = AuthorizationItems;

        if (OpenAppId.HasValue)
        {
            query = query.Where(x => x.OpenAppId == OpenAppId.Value);
        }

        if (Guid.TryParse(SelectedVehicleIdText, out var vehicleId))
        {
            query = query.Where(x => x.VehicleId == vehicleId);
        }

        FilteredItems = query
            .OrderByDescending(x => x.CreationTime)
            .ToList();
    }

    private void MergeItem(OpenAppVehicleAuthorizationDto item)
    {
        var index = AuthorizationItems.FindIndex(x => x.Id == item.Id);
        if (index >= 0)
        {
            AuthorizationItems[index] = item;
        }
        else
        {
            AuthorizationItems.Insert(0, item);
        }

        ApplyFilter();
    }

    private static DateTime? ParseDateTimeLocal(string? value)
    {
        return DateTime.TryParse(value, out var result) ? result : null;
    }

    protected sealed class AuthorizationCreateViewModel
    {
        public string? OpenAppIdText { get; set; }

        public string? VehicleIdText { get; set; }

        public DateTime AuthorizationStartTime { get; set; }

        public DateTime AuthorizationEndTime { get; set; }

        public static AuthorizationCreateViewModel CreateDefault(Guid? openAppId = null)
        {
            return new AuthorizationCreateViewModel
            {
                OpenAppIdText = openAppId?.ToString(),
                AuthorizationStartTime = DateTime.Now,
                AuthorizationEndTime = DateTime.Now.AddDays(30)
            };
        }
    }

    protected sealed class AuthorizationRenewViewModel
    {
        public DateTime AuthorizationStartTime { get; set; }

        public DateTime AuthorizationEndTime { get; set; }

        public static AuthorizationRenewViewModel CreateDefault()
        {
            return new AuthorizationRenewViewModel
            {
                AuthorizationStartTime = DateTime.Now,
                AuthorizationEndTime = DateTime.Now.AddDays(30)
            };
        }

        public static AuthorizationRenewViewModel FromDto(OpenAppVehicleAuthorizationDto dto)
        {
            return new AuthorizationRenewViewModel
            {
                AuthorizationStartTime = dto.AuthorizationStartTime,
                AuthorizationEndTime = dto.AuthorizationEndTime ?? dto.AuthorizationStartTime
            };
        }
    }

    protected sealed class AuthorizationCancelViewModel
    {
        public DateTime? CancelTime { get; set; }

        public static AuthorizationCancelViewModel CreateDefault()
        {
            return new AuthorizationCancelViewModel
            {
                CancelTime = DateTime.Now
            };
        }
    }
}
