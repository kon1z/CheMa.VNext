using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheMa.VNext.OpenPlatform;
using CheMa.VNext.Permissions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CheMa.VNext.Blazor.Client.Pages;

public partial class OpenPlatformApps
{
    private const int PageSize = 10;
    private const string OpenPolicyName = OpenPlatformPermissions.GroupName;
    private const string CreatePolicyName = OpenPlatformPermissions.OpenApps.Create;
    private const string UpdatePolicyName = OpenPlatformPermissions.OpenApps.Update;
    private const string EnablePolicyName = OpenPlatformPermissions.OpenApps.Enable;
    private const string DisablePolicyName = OpenPlatformPermissions.OpenApps.Disable;
    private const string ResetSecretPolicyName = OpenPlatformPermissions.OpenApps.ResetSecret;

    protected List<OpenAppDto> OpenApps { get; set; } = [];

    protected long TotalCount { get; set; }

    protected string? Filter { get; set; }

    protected string? SelectedStatus { get; set; }

    protected OpenAppEditViewModel EditModel { get; set; } = new();

    protected Guid? EditingId { get; set; }

    protected string CurrentSecret { get; set; } = string.Empty;

    protected bool IsEditModalVisible { get; set; }

    protected bool IsSecretModalVisible { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    protected async Task LoadAsync()
    {
        var result = await OpenAppAppService.GetListAsync(CreateListInput(0, PageSize));
        OpenApps = result.Items.ToList();
        TotalCount = result.TotalCount;
    }

    protected Task OpenCreateModalAsync()
    {
        EditingId = null;
        EditModel = new OpenAppEditViewModel();
        IsEditModalVisible = true;
        return Task.CompletedTask;
    }

    protected Task OpenEditModalAsync(OpenAppDto item)
    {
        EditingId = item.Id;
        EditModel = new OpenAppEditViewModel
        {
            Name = item.Name,
            BeginTime = item.BeginTime,
            EndTime = item.EndTime,
            AllowedIpRanges = item.AllowedIpRanges,
            Description = item.Description
        };
        IsEditModalVisible = true;
        return Task.CompletedTask;
    }

    protected Task CloseEditModalAsync()
    {
        IsEditModalVisible = false;
        return Task.CompletedTask;
    }

    protected async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(EditModel.Name))
        {
            await UiMessageService.Warn(L["OpenPlatform:Name"] + " is required.");
            return;
        }

        if (EditingId.HasValue)
        {
            await OpenAppAppService.UpdateAsync(EditingId.Value, new UpdateOpenAppDto
            {
                Name = EditModel.Name,
                BeginTime = EditModel.BeginTime,
                EndTime = EditModel.EndTime,
                AllowedIpRanges = EditModel.AllowedIpRanges,
                Description = EditModel.Description
            });
        }
        else
        {
            var result = await OpenAppAppService.CreateAsync(new CreateOpenAppDto
            {
                Name = EditModel.Name,
                BeginTime = EditModel.BeginTime,
                EndTime = EditModel.EndTime,
                AllowedIpRanges = EditModel.AllowedIpRanges,
                Description = EditModel.Description
            });

            CurrentSecret = result.AppSecret;
            IsSecretModalVisible = true;
        }

        await CloseEditModalAsync();
        await UiMessageService.Success(L["OpenPlatform:OperationSucceeded"]);
        await LoadAsync();
    }

    protected async Task EnableAsync(OpenAppDto item)
    {
        if (!await UiMessageService.Confirm(L["OpenPlatform:ConfirmEnable"]))
        {
            return;
        }

        await OpenAppAppService.EnableAsync(item.Id);
        await UiMessageService.Success(L["OpenPlatform:OperationSucceeded"]);
        await LoadAsync();
    }

    protected async Task DisableAsync(OpenAppDto item)
    {
        if (!await UiMessageService.Confirm(L["OpenPlatform:ConfirmDisable"]))
        {
            return;
        }

        await OpenAppAppService.DisableAsync(item.Id);
        await UiMessageService.Success(L["OpenPlatform:OperationSucceeded"]);
        await LoadAsync();
    }

    protected async Task ResetSecretAsync(OpenAppDto item)
    {
        if (!await UiMessageService.Confirm(L["OpenPlatform:ConfirmResetSecret"]))
        {
            return;
        }

        var result = await OpenAppAppService.ResetSecretAsync(item.Id);
        CurrentSecret = result.AppSecret;
        IsSecretModalVisible = true;
        await UiMessageService.Success(L["OpenPlatform:OperationSucceeded"]);
        await LoadAsync();
    }

    protected Task CloseSecretModalAsync()
    {
        IsSecretModalVisible = false;
        return Task.CompletedTask;
    }

    protected void OnFilterInput(ChangeEventArgs args)
    {
        Filter = args.Value?.ToString();
    }

    protected void OnStatusChanged(ChangeEventArgs args)
    {
        SelectedStatus = args.Value?.ToString();
    }

    protected void OnNameInput(ChangeEventArgs args)
    {
        EditModel.Name = args.Value?.ToString() ?? string.Empty;
    }

    protected void OnAllowedIpRangesInput(ChangeEventArgs args)
    {
        EditModel.AllowedIpRanges = args.Value?.ToString();
    }

    protected void OnDescriptionInput(ChangeEventArgs args)
    {
        EditModel.Description = args.Value?.ToString();
    }

    protected void OnBeginTimeChanged(ChangeEventArgs args)
    {
        EditModel.BeginTime = ParseDateTimeLocal(args.Value?.ToString());
    }

    protected void OnEndTimeChanged(ChangeEventArgs args)
    {
        EditModel.EndTime = ParseDateTimeLocal(args.Value?.ToString());
    }

    protected string FormatDateTime(DateTime? value)
    {
        return value?.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") ?? "-";
    }

    protected string ToDateTimeLocalValue(DateTime? value)
    {
        return value?.ToLocalTime().ToString("yyyy-MM-ddTHH:mm") ?? string.Empty;
    }

    private GetOpenAppListInput CreateListInput(int skipCount, int maxResultCount)
    {
        return new GetOpenAppListInput
        {
            Filter = Filter,
            Status = ParseStatus(),
            SkipCount = skipCount,
            MaxResultCount = maxResultCount,
            Sorting = nameof(OpenAppDto.CreationTime) + " DESC"
        };
    }

    private OpenAppStatus? ParseStatus()
    {
        return SelectedStatus switch
        {
            nameof(OpenAppStatus.Enabled) => OpenAppStatus.Enabled,
            nameof(OpenAppStatus.Disabled) => OpenAppStatus.Disabled,
            _ => null
        };
    }

    private static DateTime? ParseDateTimeLocal(string? value)
    {
        return DateTime.TryParse(value, out var result) ? result : null;
    }

    protected class OpenAppEditViewModel
    {
        public string Name { get; set; } = string.Empty;

        public DateTime? BeginTime { get; set; }

        public DateTime? EndTime { get; set; }

        public string? AllowedIpRanges { get; set; }

        public string? Description { get; set; }
    }
}