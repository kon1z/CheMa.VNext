using System.Threading;
using System.Threading.Tasks;

namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿车联网开放接口网关。
/// </summary>
public interface IMaiHongGateway
{
    /// <summary>
    /// 查询品牌。
    /// </summary>
    Task<MaiHongResponse<MaiHongBrandDto[]>> GetBrandsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 查询车系。
    /// </summary>
    Task<MaiHongResponse<MaiHongStyleDto[]>> GetStylesAsync(string brandId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 查询车型。
    /// </summary>
    Task<MaiHongResponse<MaiHongModelDto[]>> GetModelsAsync(string styleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 新增车辆。
    /// </summary>
    Task<MaiHongAddVehicleResponse> AddVehicleAsync(MaiHongVehicleCreateRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 修改车辆。
    /// </summary>
    Task<MaiHongResponse> UpdateVehicleAsync(string vehicleId, MaiHongVehicleUpdateRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 查询车辆。
    /// </summary>
    Task<MaiHongResponse<MaiHongVehicleDto>> GetVehicleAsync(string vehicleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除车辆。
    /// </summary>
    Task<MaiHongResponse> DeleteVehicleAsync(string vehicleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 车辆控制。
    /// </summary>
    Task<MaiHongResponse> ControlVehicleAsync(MaiHongVehicleControlRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 车辆定位。
    /// </summary>
    Task<MaiHongResponse<MaiHongPositionsDataDto>> GetPositionsAsync(string vehicleHwid, CancellationToken cancellationToken = default);

    /// <summary>
    /// 查询车身状态。
    /// </summary>
    Task<MaiHongResponse<MaiHongVehicleStatusDto[]>> GetVehicleStatusAsync(string vehicleHwid, CancellationToken cancellationToken = default);

    /// <summary>
    /// 行程查询。
    /// </summary>
    Task<MaiHongResponse<MaiHongTripsDataDto>> GetTripsAsync(MaiHongTripsQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// 行车轨迹。
    /// </summary>
    Task<MaiHongResponse<MaiHongTracesDataDto>> GetTracesAsync(MaiHongTracesQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// 告警类型查询。
    /// </summary>
    Task<MaiHongResponse<MaiHongAlertTypesDataDto>> GetAlertTypesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 告警历史查询。
    /// </summary>
    Task<MaiHongResponse<MaiHongAlertsDataDto>> GetAlertsAsync(MaiHongAlertsQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// 查询车辆参数。
    /// </summary>
    Task<MaiHongSettingResponse> SearchSettingsAsync(string vehicleHwid, string? parameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置车辆参数。
    /// </summary>
    Task<MaiHongSettingResultResponse> SetSettingsAsync(string vehicleHwid, MaiHongVehicleSettingRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 查询蓝牙设备信息。
    /// </summary>
    Task<MaiHongBluetoothInfoResponse> GetBluetoothInfoAsync(string vehicleHwid, CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置和修改蓝牙设备密码。
    /// </summary>
    Task<MaiHongBluetoothSetPasswordResponse> SetBluetoothPasswordAsync(MaiHongSetBluetoothPasswordRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 修改蓝牙设备名称。
    /// </summary>
    Task<MaiHongCommandResultResponse> SetBluetoothNameAsync(MaiHongSetBluetoothNameRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 激活车辆。
    /// </summary>
    Task<MaiHongCommandResultResponse> ActiveVehicleAsync(string vehicleHwid, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取车辆有效结束时间。
    /// </summary>
    Task<MaiHongEquipEndTimeResponse> GetEquipEndTimeAsync(string vehicleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置 GPS 设备扣费开关。
    /// </summary>
    Task<MaiHongResponse> SetServiceSwitchByEquipmentCodeAsync(string equipmentCode, int serviceSwitch, CancellationToken cancellationToken = default);

    /// <summary>
    /// 按车辆 ID 设置 GPS 设备扣费开关。
    /// </summary>
    Task<MaiHongResponse> SetServiceSwitchByVehicleIdAsync(string vehicleId, int serviceSwitch, CancellationToken cancellationToken = default);
}
