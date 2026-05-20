using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CheMa.VNext.VehicleDevices.Dtos;
using CheMa.VNext.VehicleDevices.Inputs;
using CheMa.VNext.Vehicles.Dtos;
using CheMa.VNext.Vehicles.Enums;
using CheMa.VNext.Vehicles.Inputs;
using Microsoft.AspNetCore.Components;

namespace CheMa.VNext.Blazor.Client.Pages;

public partial class Vehicles
{
    private static readonly VehicleDeviceVendorType[] VendorTypeOptions =
    [
        VehicleDeviceVendorType.MaiHong,
        VehicleDeviceVendorType.Other
    ];
    private List<VehicleDto> VehicleItems { get; set; } = [];
    private string? Filter { get; set; }
    private bool IsEditModalVisible { get; set; }
    private bool IsBindModalVisible { get; set; }
    private Guid? EditingId { get; set; }
    private Guid? BindingVehicleId { get; set; }
    private string BindingVehicleVin { get; set; } = string.Empty;
    private VehicleEditModel EditModel { get; set; } = VehicleEditModel.CreateDefault();
    private VehicleBindModel BindModel { get; set; } = VehicleBindModel.CreateDefault();
    private List<VehicleBrandDto> BrandOptions { get; set; } = [];
    private List<VehicleStyleDto> StyleOptions { get; set; } = [];
    private List<VehicleModelDto> ModelOptions { get; set; } = [];
    private string? BrandFilter { get; set; }
    private string? StyleFilter { get; set; }
    private string? ModelFilter { get; set; }
    private IEnumerable<VehicleBrandDto> FilteredBrandOptions => BrandOptions.Where(option => MatchesOptionFilter(option.Name, option.Id, BrandFilter));
    private IEnumerable<VehicleStyleDto> FilteredStyleOptions => StyleOptions.Where(option => MatchesOptionFilter(option.Name, option.Id, StyleFilter));
    private IEnumerable<VehicleModelDto> FilteredModelOptions => ModelOptions.Where(option => MatchesOptionFilter(option.Name, option.Id, ModelFilter));
    private bool IsLoadingBrands { get; set; }
    private bool IsLoadingStyles { get; set; }
    private bool IsLoadingModels { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }
    private async Task LoadAsync()
    {
        var result = await VehicleAppService.GetListAsync(new GetVehicleListInput
        {
            Filter = Filter,
            MaxResultCount = 100,
            SkipCount = 0,
            Sorting = nameof(VehicleDto.CreationTime) + " desc"
        });

        VehicleItems = result.Items.ToList();
    }

    private Task OpenCreateModalAsync()
    {
        EditingId = null;
        EditModel = VehicleEditModel.CreateDefault();
        IsEditModalVisible = true;
        return Task.CompletedTask;
    }

    private Task OpenEditModalAsync(VehicleDto item)
    {
        EditingId = item.Id;
        EditModel = VehicleEditModel.FromDto(item);
        IsEditModalVisible = true;
        return Task.CompletedTask;
    }

    private async Task OpenBindModalAsync(VehicleDto item)
    {
        BindingVehicleId = item.Id;
        BindingVehicleVin = item.Vin;
        BindModel = VehicleBindModel.CreateDefault(item.VendorType);
        ClearVehicleSelectionOptions();
        IsBindModalVisible = true;

        if (BindModel.VendorType == VehicleDeviceVendorType.MaiHong)
        {
            await LoadBrandsAsync();
        }
    }

    private Task CloseEditModalAsync()
    {
        IsEditModalVisible = false;
        EditingId = null;
        EditModel = VehicleEditModel.CreateDefault();
        return Task.CompletedTask;
    }

    private Task CloseBindModalAsync()
    {
        IsBindModalVisible = false;
        BindingVehicleId = null;
        BindingVehicleVin = string.Empty;
        BindModel = VehicleBindModel.CreateDefault();
        ClearVehicleSelectionOptions();
        return Task.CompletedTask;
    }

    private async Task SaveAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(EditModel.EngineNumber))
            {
                await UiMessageService.Warn("请输入发动机编号");
                return;
            }

            if (EditingId.HasValue)
            {
                await VehicleAppService.UpdateAsync(EditingId.Value, EditModel.ToUpdateDto());
            }
            else
            {
                await VehicleAppService.CreateAsync(EditModel.ToCreateDto());
            }

            await UiMessageService.Success("保存成功");
            await CloseEditModalAsync();
            await LoadAsync();
        }
        catch (Exception ex)
        {
            await UiMessageService.Error(ex.Message);
        }
    }

    private async Task BindAsync()
    {
        if (!BindingVehicleId.HasValue)
        {
            return;
        }

        var vendorDeviceId = BindModel.VendorDeviceId.Trim();
        if (BindModel.VendorType == VehicleDeviceVendorType.MaiHong &&
            (string.IsNullOrWhiteSpace(BindModel.BrandId) ||
             string.IsNullOrWhiteSpace(BindModel.StyleId) ||
             string.IsNullOrWhiteSpace(BindModel.ModelId)))
        {
            await UiMessageService.Warn("请选择车辆品牌、车系、车型");
            return;
        }

        if (string.IsNullOrWhiteSpace(vendorDeviceId))
        {
            await UiMessageService.Warn("请输入供应商设备编号");
            return;
        }

        try
        {
            await VehicleDeviceAppService.BindAsync(new BindVehicleDeviceInput
            {
                VehicleId = BindingVehicleId.Value,
                VendorType = BindModel.VendorType,
                VendorDeviceId = vendorDeviceId,
                BrandId = BindModel.BrandId,
                StyleId = BindModel.StyleId,
                ModelId = BindModel.ModelId
            });

            await UiMessageService.Success("绑定成功");
            await CloseBindModalAsync();
            await LoadAsync();
        }
        catch (Exception ex)
        {
            await UiMessageService.Error(ex.Message);
        }
    }

    private async Task UnbindAsync(VehicleDto item)
    {
        var confirmed = await UiMessageService.Confirm($"确认解绑车辆 {item.Vin} 的车机吗？");
        if (!confirmed)
        {
            return;
        }

        try
        {
            await VehicleDeviceAppService.UnbindAsync(item.Id);
            await UiMessageService.Success("解绑成功");
            await LoadAsync();
        }
        catch (Exception ex)
        {
            await UiMessageService.Error(ex.Message);
        }
    }

    private async Task DeleteAsync(VehicleDto item)
    {
        var confirmed = await UiMessageService.Confirm($"确认删除车辆 {item.Vin} 吗？");
        if (!confirmed)
        {
            return;
        }

        try
        {
            await VehicleAppService.DeleteAsync(item.Id);
            await UiMessageService.Success("删除成功");
            await LoadAsync();
        }
        catch (Exception ex)
        {
            await UiMessageService.Error(ex.Message);
        }
    }

    private void OnFilterInput(ChangeEventArgs args)
    {
        Filter = args.Value?.ToString();
    }

    private void OnVinInput(ChangeEventArgs args)
    {
        EditModel.Vin = args.Value?.ToString() ?? string.Empty;
    }

    private void OnPlateNumberInput(ChangeEventArgs args)
    {
        EditModel.PlateNumber = args.Value?.ToString();
    }

    private void OnEngineNumberInput(ChangeEventArgs args)
    {
        EditModel.EngineNumber = args.Value?.ToString() ?? string.Empty;
    }

    private async Task OnBindVendorTypeChanged(ChangeEventArgs args)
    {
        if (int.TryParse(args.Value?.ToString(), out var value) &&
            Enum.IsDefined(typeof(VehicleDeviceVendorType), value))
        {
            BindModel.VendorType = (VehicleDeviceVendorType)value;
            BindModel.BrandId = null;
            BindModel.StyleId = null;
            BindModel.ModelId = null;
            ClearVehicleSelectionOptions();

            if (BindModel.VendorType == VehicleDeviceVendorType.MaiHong)
            {
                await LoadBrandsAsync();
            }
        }
    }

    private async Task OnBindBrandChanged(ChangeEventArgs args)
    {
        BindModel.BrandId = args.Value?.ToString();
        BindModel.StyleId = null;
        BindModel.ModelId = null;
        StyleOptions = [];
        ModelOptions = [];
        StyleFilter = null;
        ModelFilter = null;

        if (!string.IsNullOrWhiteSpace(BindModel.BrandId))
        {
            await LoadStylesAsync(BindModel.BrandId);
        }
    }

    private async Task OnBindStyleChanged(ChangeEventArgs args)
    {
        BindModel.StyleId = args.Value?.ToString();
        BindModel.ModelId = null;
        ModelOptions = [];
        ModelFilter = null;

        if (!string.IsNullOrWhiteSpace(BindModel.StyleId))
        {
            await LoadModelsAsync(BindModel.StyleId);
        }
    }

    private void OnBindModelChanged(ChangeEventArgs args)
    {
        BindModel.ModelId = args.Value?.ToString();
    }

    private void OnBrandFilterInput(ChangeEventArgs args)
    {
        BrandFilter = args.Value?.ToString();
    }

    private void OnStyleFilterInput(ChangeEventArgs args)
    {
        StyleFilter = args.Value?.ToString();
    }

    private void OnModelFilterInput(ChangeEventArgs args)
    {
        ModelFilter = args.Value?.ToString();
    }

    private void OnVendorDeviceIdInput(ChangeEventArgs args)
    {
        BindModel.VendorDeviceId = args.Value?.ToString() ?? string.Empty;
    }

    private async Task LoadBrandsAsync()
    {
        try
        {
            IsLoadingBrands = true;
            BrandOptions = await VehicleDeviceAppService.GetBrandsAsync();
        }
        catch (Exception ex)
        {
            await UiMessageService.Error(ex.Message);
        }
        finally
        {
            IsLoadingBrands = false;
        }
    }

    private async Task LoadStylesAsync(string brandId)
    {
        try
        {
            IsLoadingStyles = true;
            StyleOptions = await VehicleDeviceAppService.GetStylesAsync(brandId);
        }
        catch (Exception ex)
        {
            await UiMessageService.Error(ex.Message);
        }
        finally
        {
            IsLoadingStyles = false;
        }
    }

    private async Task LoadModelsAsync(string styleId)
    {
        try
        {
            IsLoadingModels = true;
            ModelOptions = await VehicleDeviceAppService.GetModelsAsync(styleId);
        }
        catch (Exception ex)
        {
            await UiMessageService.Error(ex.Message);
        }
        finally
        {
            IsLoadingModels = false;
        }
    }

    private void ClearVehicleSelectionOptions()
    {
        BrandOptions = [];
        StyleOptions = [];
        ModelOptions = [];
        BrandFilter = null;
        StyleFilter = null;
        ModelFilter = null;
        IsLoadingBrands = false;
        IsLoadingStyles = false;
        IsLoadingModels = false;
    }

    private static bool MatchesOptionFilter(string? name, string id, string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return true;
        }

        var keyword = filter.Trim();
        return id.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
               (!string.IsNullOrWhiteSpace(name) && name.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private static string GetOptionText(string? name, string id)
    {
        return string.IsNullOrWhiteSpace(name) ? id : $"{name}（{id}）";
    }

    private static string GetVendorTypeText(VehicleDeviceVendorType? deviceType)
    {
        return deviceType switch
        {
            VehicleDeviceVendorType.MaiHong => "迈鸿车机",
            VehicleDeviceVendorType.Other => "其他供应商车机",
            _ => "-"
        };
    }

    private static string GetBindingStatusText(VehicleBindingStatus bindingStatus)
    {
        return bindingStatus switch
        {
            VehicleBindingStatus.Bound => "已绑定",
            _ => "未绑定"
        };
    }

    private static string GetBindingDeviceSummary(VehicleDto item)
    {
        if (item.BindingStatus != VehicleBindingStatus.Bound || item.VendorType == null)
        {
            return "未绑定设备";
        }

        return GetVendorTypeText(item.VendorType);
    }

    private static string FormatDateTime(DateTime? value)
    {
        return value?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-";
    }

    private sealed class VehicleEditModel
    {
        public string Vin { get; set; } = string.Empty;

        public string? PlateNumber { get; set; }

        public string EngineNumber { get; set; } = string.Empty;

        public VehicleDeviceVendorType? VendorType { get; set; }

        public VehicleBindingStatus BindingStatus { get; set; }

        public DateTime? BindingTime { get; set; }

        public static VehicleEditModel CreateDefault()
        {
            return new VehicleEditModel
            {
                VendorType = null, BindingStatus = VehicleBindingStatus.Unbound, BindingTime = null
            };
        }

        public static VehicleEditModel FromDto(VehicleDto dto)
        {
            return new VehicleEditModel
            {
                Vin = dto.Vin,
                PlateNumber = dto.PlateNumber,
                EngineNumber = dto.EngineNumber,
                VendorType = dto.VendorType,
                BindingStatus = dto.BindingStatus,
                BindingTime = dto.BindingTime
            };
        }

        public CreateVehicleDto ToCreateDto()
        {
            return new CreateVehicleDto
            {
                Vin = Vin,
                PlateNumber = PlateNumber,
                EngineNumber = EngineNumber,
                VendorType = null,
                BindingStatus = VehicleBindingStatus.Unbound,
                BindingTime = null
            };
        }

        public UpdateVehicleDto ToUpdateDto()
        {
            return new UpdateVehicleDto
            {
                Vin = Vin,
                PlateNumber = PlateNumber,
                EngineNumber = EngineNumber,
                VendorType = VendorType,
                BindingStatus = BindingStatus,
                BindingTime = BindingTime
            };
        }
    }

    private sealed class VehicleBindModel
    {
        public VehicleDeviceVendorType VendorType { get; set; }

        public string VendorDeviceId { get; set; } = string.Empty;

        public string? BrandId { get; set; }

        public string? StyleId { get; set; }

        public string? ModelId { get; set; }

        public static VehicleBindModel CreateDefault(VehicleDeviceVendorType? deviceType = null)
        {
            return new VehicleBindModel
            {
                VendorType = deviceType ?? VehicleDeviceVendorType.MaiHong, VendorDeviceId = string.Empty
            };
        }
    }
}
