using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CheMa.VNext.Models.MaiHong;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CheMa.VNext.Controllers.MaiHong;

/// <summary>
/// 迈鸿事件回调接口。
/// </summary>
[Route("api/maihong/events")]
public class MaiHongCallbackController : VNextController
{
    private readonly ILogger<MaiHongCallbackController> _logger;

    /// <summary>
    /// 初始化迈鸿事件回调接口。
    /// </summary>
    public MaiHongCallbackController(ILogger<MaiHongCallbackController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 接收迈鸿事件回调。
    /// </summary>
    [HttpPost("callback")]
    public Task<MaiHongCallbackResponse> CallbackAsync([FromBody] MaiHongEventCallbackDto input)
    {
        _logger.LogInformation(
            "MaiHong event callback received. Type: {Type}, SubType: {SubType}, VehicleHwid: {VehicleHwid}, Time: {Time}",
            input.Type,
            input.SubType,
            input.Object,
            input.Time);

        return Task.FromResult(new MaiHongCallbackResponse());
    }
}

/// <summary>
/// 迈鸿回调响应。
/// </summary>
public class MaiHongCallbackResponse
{
    /// <summary>
    /// 错误码。
    /// </summary>
    [JsonPropertyName("errno")]
    public int Errno { get; set; }

    /// <summary>
    /// 错误信息。
    /// </summary>
    [JsonPropertyName("error")]
    public string Error { get; set; } = "success";
}
