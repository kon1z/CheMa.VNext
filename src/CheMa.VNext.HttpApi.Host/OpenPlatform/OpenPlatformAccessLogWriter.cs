using System;
using System.Threading.Tasks;
using CheMa.VNext.OpenPlatform.Entities;
using Microsoft.Extensions.Logging;
using Volo.Abp.Domain.Repositories;

namespace CheMa.VNext.OpenPlatform;

public class OpenPlatformAccessLogWriter
{
    private readonly IRepository<OpenApiAccessLog, Guid> _openApiAccessLogRepository;
    private readonly IRepository<OpenApp, Guid> _openAppRepository;
    private readonly ILogger<OpenPlatformAccessLogWriter> _logger;

    public OpenPlatformAccessLogWriter(
        IRepository<OpenApiAccessLog, Guid> openApiAccessLogRepository,
        IRepository<OpenApp, Guid> openAppRepository,
        ILogger<OpenPlatformAccessLogWriter> logger)
    {
        _openApiAccessLogRepository = openApiAccessLogRepository;
        _openAppRepository = openAppRepository;
        _logger = logger;
    }

    public async Task WriteAsync(OpenPlatformAccessLogInfo info)
    {
        var log = new OpenApiAccessLog(
            Guid.NewGuid(),
            info.OpenAppId,
            info.ClientId,
            info.RequestPath,
            info.HttpMethod,
            info.QueryString,
            info.TraceId,
            info.RemoteIpAddress,
            info.UserAgent,
            info.Timestamp,
            info.ElapsedMs,
            info.Succeeded,
            info.FailureCode,
            info.FailureMessage,
            info.ResponseStatusCode);

        await _openApiAccessLogRepository.InsertAsync(log, autoSave: true);

        if (info.OpenAppId.HasValue && info.Succeeded)
        {
            var openApp = await _openAppRepository.GetAsync(info.OpenAppId.Value);
            openApp.UpdateLastAccess(info.Timestamp, info.RemoteIpAddress);
            await _openAppRepository.UpdateAsync(openApp, autoSave: true);
        }

        _logger.LogInformation(
            "Open platform request {ClientId} {RequestPath} {HttpMethod} completed with success={Succeeded}, statusCode={StatusCode}, elapsedMs={ElapsedMs}, failureCode={FailureCode}",
            info.ClientId,
            info.RequestPath,
            info.HttpMethod,
            info.Succeeded,
            info.ResponseStatusCode,
            info.ElapsedMs,
            info.FailureCode);
    }
}

public class OpenPlatformAccessLogInfo
{
    public Guid? OpenAppId { get; init; }

    public string? ClientId { get; init; }

    public string RequestPath { get; init; } = string.Empty;

    public string HttpMethod { get; init; } = string.Empty;

    public string? QueryString { get; init; }

    public string? TraceId { get; init; }

    public string? RemoteIpAddress { get; init; }

    public string? UserAgent { get; init; }

    public DateTime Timestamp { get; init; }

    public long ElapsedMs { get; init; }

    public bool Succeeded { get; init; }

    public string? FailureCode { get; init; }

    public string? FailureMessage { get; init; }

    public int? ResponseStatusCode { get; init; }
}
