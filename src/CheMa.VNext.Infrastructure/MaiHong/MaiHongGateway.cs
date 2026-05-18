using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace CheMa.VNext.MaiHong;

/// <summary>
/// 迈鸿车联网开放接口 HTTP 网关。
/// </summary>
public class MaiHongGateway : IMaiHongGateway
{
    private const string AuthorizationHeader = "WSSE realm=mhcs, profile=UsernameToken, type=Username";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly MaiHongGatewayOptions _options;

    /// <summary>
    /// 初始化迈鸿车联网开放接口 HTTP 网关。
    /// </summary>
    public MaiHongGateway(HttpClient httpClient, IOptions<MaiHongGatewayOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    /// <inheritdoc />
    public Task<MaiHongResponse<MaiHongBrandDto[]>> GetBrandsAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<MaiHongResponse<MaiHongBrandDto[]>>("/web/tpc/api/brands", null, cancellationToken);
    }

    /// <inheritdoc />
    public Task<MaiHongResponse<MaiHongStyleDto[]>> GetStylesAsync(string brandId, CancellationToken cancellationToken = default)
    {
        return GetAsync<MaiHongResponse<MaiHongStyleDto[]>>("/web/tpc/api/styles", new Dictionary<string, string?>
        {
            ["brandId"] = brandId
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task<MaiHongResponse<MaiHongModelDto[]>> GetModelsAsync(string styleId, CancellationToken cancellationToken = default)
    {
        return GetAsync<MaiHongResponse<MaiHongModelDto[]>>("/web/tpc/api/models", new Dictionary<string, string?>
        {
            ["styleId"] = styleId
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task<MaiHongAddVehicleResponse> AddVehicleAsync(MaiHongVehicleCreateRequest request, CancellationToken cancellationToken = default)
    {
        return PostAsync<MaiHongAddVehicleResponse>("/web/tpc/api/vehicle/add", request, null, cancellationToken);
    }

    /// <inheritdoc />
    public Task<MaiHongResponse> UpdateVehicleAsync(string vehicleId, MaiHongVehicleUpdateRequest request, CancellationToken cancellationToken = default)
    {
        return PostAsync<MaiHongResponse>("/web/tpc/api/vehicle/update", request, new Dictionary<string, string?>
        {
            ["vehicleId"] = vehicleId
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task<MaiHongResponse<MaiHongVehicleDto>> GetVehicleAsync(string vehicleId, CancellationToken cancellationToken = default)
    {
        return GetAsync<MaiHongResponse<MaiHongVehicleDto>>("/web/tpc/api/vehicle/searchForSingle", new Dictionary<string, string?>
        {
            ["vehicleId"] = vehicleId
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task<MaiHongResponse> DeleteVehicleAsync(string vehicleId, CancellationToken cancellationToken = default)
    {
        return GetAsync<MaiHongResponse>("/web/tpc/api/vehicle/delete", new Dictionary<string, string?>
        {
            ["vehicleId"] = vehicleId
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task<MaiHongResponse> ControlVehicleAsync(MaiHongVehicleControlRequest request, CancellationToken cancellationToken = default)
    {
        return PostAsync<MaiHongResponse>("/web/tpc/api/vehicle/controls", request, null, cancellationToken);
    }

    /// <inheritdoc />
    public Task<MaiHongResponse<MaiHongPositionsDataDto>> GetPositionsAsync(string vehicleHwid, CancellationToken cancellationToken = default)
    {
        return GetAsync<MaiHongResponse<MaiHongPositionsDataDto>>("/web/tpc/api/vehicle/positions", VehicleHwidQuery(vehicleHwid), cancellationToken);
    }

    /// <inheritdoc />
    public Task<MaiHongResponse<MaiHongVehicleStatusDto[]>> GetVehicleStatusAsync(string vehicleHwid, CancellationToken cancellationToken = default)
    {
        return GetAsync<MaiHongResponse<MaiHongVehicleStatusDto[]>>("/web/tpc/api/vehicle/status", VehicleHwidQuery(vehicleHwid), cancellationToken);
    }

    /// <inheritdoc />
    public Task<MaiHongResponse<MaiHongTripsDataDto>> GetTripsAsync(MaiHongTripsQuery query, CancellationToken cancellationToken = default)
    {
        return GetAsync<MaiHongResponse<MaiHongTripsDataDto>>("/web/tpc/api/vehicle/trips", new Dictionary<string, string?>
        {
            ["vehicleHwid"] = query.VehicleHwid,
            ["date_from"] = query.DateFrom,
            ["date_to"] = query.DateTo
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task<MaiHongResponse<MaiHongTracesDataDto>> GetTracesAsync(MaiHongTracesQuery query, CancellationToken cancellationToken = default)
    {
        return GetAsync<MaiHongResponse<MaiHongTracesDataDto>>("/web/tpc/api/vehicle/traces", new Dictionary<string, string?>
        {
            ["vehicleHwid"] = query.VehicleHwid,
            ["type"] = query.Type.ToString(),
            ["trip_id"] = query.TripId,
            ["date_from"] = query.DateFrom,
            ["date_to"] = query.DateTo
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task<MaiHongResponse<MaiHongAlertTypesDataDto>> GetAlertTypesAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<MaiHongResponse<MaiHongAlertTypesDataDto>>("/web/tpc/api/vehicle/alerts/types", null, cancellationToken);
    }

    /// <inheritdoc />
    public Task<MaiHongResponse<MaiHongAlertsDataDto>> GetAlertsAsync(MaiHongAlertsQuery query, CancellationToken cancellationToken = default)
    {
        return GetAsync<MaiHongResponse<MaiHongAlertsDataDto>>("/web/api/vehicle/alerts", new Dictionary<string, string?>
        {
            ["vehicleHwid"] = query.VehicleHwid,
            ["page"] = query.Page.ToString(),
            ["pageSize"] = query.PageSize?.ToString(),
            ["date_from"] = query.DateFrom,
            ["date_to"] = query.DateTo,
            ["order"] = query.Order?.ToString(),
            ["alertType"] = query.AlertType
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task<MaiHongSettingResponse> SearchSettingsAsync(string vehicleHwid, string? parameters = null, CancellationToken cancellationToken = default)
    {
        return GetAsync<MaiHongSettingResponse>("/web/tpc/api/vehicle/searchSetting", new Dictionary<string, string?>
        {
            ["vehicleHwid"] = vehicleHwid,
            ["params"] = parameters
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task<MaiHongSettingResultResponse> SetSettingsAsync(string vehicleHwid, MaiHongVehicleSettingRequest request, CancellationToken cancellationToken = default)
    {
        return PostAsync<MaiHongSettingResultResponse>("/web/tpc/api/vehicles/settings", request, new Dictionary<string, string?>
        {
            ["vehicleHwid"] = vehicleHwid,
            ["jsonStr"] = JsonSerializer.Serialize(request, JsonOptions)
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task<MaiHongBluetoothInfoResponse> GetBluetoothInfoAsync(string vehicleHwid, CancellationToken cancellationToken = default)
    {
        return PostAsync<MaiHongBluetoothInfoResponse>("/web/tpc/api/vehicles/getBluetoothInfo", null, VehicleHwidQuery(vehicleHwid), cancellationToken);
    }

    /// <inheritdoc />
    public Task<MaiHongBluetoothSetPasswordResponse> SetBluetoothPasswordAsync(MaiHongSetBluetoothPasswordRequest request, CancellationToken cancellationToken = default)
    {
        return PostAsync<MaiHongBluetoothSetPasswordResponse>("/web/tpc/api/vehicles/setBluetoothInfo", request, null, cancellationToken);
    }

    /// <inheritdoc />
    public Task<MaiHongCommandResultResponse> SetBluetoothNameAsync(MaiHongSetBluetoothNameRequest request, CancellationToken cancellationToken = default)
    {
        return PostAsync<MaiHongCommandResultResponse>("/web/tpc/api/vehicles/setBluetoothName", request, null, cancellationToken);
    }

    /// <inheritdoc />
    public Task<MaiHongCommandResultResponse> ActiveVehicleAsync(string vehicleHwid, CancellationToken cancellationToken = default)
    {
        return PostAsync<MaiHongCommandResultResponse>("/web/tpc/api/vehicles/activeVehicle", null, VehicleHwidQuery(vehicleHwid), cancellationToken);
    }

    /// <inheritdoc />
    public Task<MaiHongEquipEndTimeResponse> GetEquipEndTimeAsync(string vehicleId, CancellationToken cancellationToken = default)
    {
        return PostAsync<MaiHongEquipEndTimeResponse>("/web/tpc/api/vehicles/getEquipEndTime", null, new Dictionary<string, string?>
        {
            ["vehicleId"] = vehicleId
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task<MaiHongResponse> SetServiceSwitchByEquipmentCodeAsync(string equipmentCode, int serviceSwitch, CancellationToken cancellationToken = default)
    {
        return PostAsync<MaiHongResponse>("/web/tpc/api/vehicles/serviceSwitchApi", null, new Dictionary<string, string?>
        {
            ["equipmentCode"] = equipmentCode,
            ["serviceSwitch"] = serviceSwitch.ToString()
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task<MaiHongResponse> SetServiceSwitchByVehicleIdAsync(string vehicleId, int serviceSwitch, CancellationToken cancellationToken = default)
    {
        return PostAsync<MaiHongResponse>("/web/tpc/api/vehicles/serviceSwitch", null, new Dictionary<string, string?>
        {
            ["vehicleId"] = vehicleId,
            ["serviceSwitch"] = serviceSwitch.ToString()
        }, cancellationToken);
    }

    private async Task<T> GetAsync<T>(string path, Dictionary<string, string?>? query, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, BuildUri(path, query));
        AddAuthenticationHeaders(request);

        return await SendAsync<T>(request, cancellationToken);
    }

    private async Task<T> PostAsync<T>(string path, object? body, Dictionary<string, string?>? query, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, BuildUri(path, query));
        AddAuthenticationHeaders(request);

        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        return await SendAsync<T>(request, cancellationToken);
    }

    private async Task<T> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        if (result is null)
        {
            throw new InvalidOperationException("MaiHong returned empty response.");
        }

        return result;
    }

    private static string BuildUri(string path, Dictionary<string, string?>? query)
    {
        if (query is null || query.Count == 0)
        {
            return path;
        }

        var builder = new StringBuilder(path);
        var hasQuery = path.Contains('?', StringComparison.Ordinal);

        foreach (var item in query)
        {
            if (item.Value is null)
            {
                continue;
            }

            builder.Append(hasQuery ? '&' : '?');
            builder.Append(Uri.EscapeDataString(item.Key));
            builder.Append('=');
            builder.Append(Uri.EscapeDataString(item.Value));
            hasQuery = true;
        }

        return builder.ToString();
    }

    private static Dictionary<string, string?> VehicleHwidQuery(string vehicleHwid)
    {
        return new Dictionary<string, string?>
        {
            ["vehicleHwid"] = vehicleHwid
        };
    }

    private void AddAuthenticationHeaders(HttpRequestMessage request)
    {
        var nonce = RandomNumberGenerator.GetInt32(0, 10000000).ToString();
        var created = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
        var digest = CreatePasswordDigest(nonce, created);

        request.Headers.TryAddWithoutValidation("Authorization", AuthorizationHeader);
        request.Headers.TryAddWithoutValidation(
            "X-WSSE",
            $"UsernameToken Username={_options.UserName}, PasswordDigest={digest}, Nonce={nonce}, Created={created}");
    }

    private string CreatePasswordDigest(string nonce, string created)
    {
        var raw = $"{nonce}{created}{_options.ApiKey}{_options.UserName}{_options.DigestUri}";
        raw = raw.Trim().Replace("\r", string.Empty).Replace("\n", string.Empty);
        var hash = SHA1.HashData(Encoding.UTF8.GetBytes(raw));

        return Convert.ToBase64String(hash);
    }
}
