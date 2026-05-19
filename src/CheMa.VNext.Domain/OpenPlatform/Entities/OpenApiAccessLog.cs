using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace CheMa.VNext.OpenPlatform.Entities;

public class OpenApiAccessLog : CreationAuditedAggregateRoot<Guid>
{
    public Guid? OpenAppId { get; private set; }

    public string? ClientId { get; private set; }

    public string RequestPath { get; private set; } = string.Empty;

    public string HttpMethod { get; private set; } = string.Empty;

    public string? QueryString { get; private set; }

    public string? TraceId { get; private set; }

    public string? RemoteIpAddress { get; private set; }

    public string? UserAgent { get; private set; }

    public DateTime Timestamp { get; private set; }

    public long ElapsedMs { get; private set; }

    public bool Succeeded { get; private set; }

    public string? FailureCode { get; private set; }

    public string? FailureMessage { get; private set; }

    public int? ResponseStatusCode { get; private set; }

    protected OpenApiAccessLog()
    {
    }

    public OpenApiAccessLog(
        Guid id,
        Guid? openAppId,
        string? clientId,
        string requestPath,
        string httpMethod,
        string? queryString,
        string? traceId,
        string? remoteIpAddress,
        string? userAgent,
        DateTime timestamp,
        long elapsedMs,
        bool succeeded,
        string? failureCode,
        string? failureMessage,
        int? responseStatusCode)
        : base(id)
    {
        OpenAppId = openAppId;
        ClientId = Check.Length(clientId, nameof(clientId), OpenPlatformConsts.MaxClientIdLength);
        RequestPath = Check.NotNullOrWhiteSpace(requestPath, nameof(requestPath), OpenPlatformConsts.MaxRequestPathLength);
        HttpMethod = Check.NotNullOrWhiteSpace(httpMethod, nameof(httpMethod), OpenPlatformConsts.MaxHttpMethodLength);
        QueryString = Check.Length(queryString, nameof(queryString), OpenPlatformConsts.MaxQueryStringLength);
        TraceId = Check.Length(traceId, nameof(traceId), OpenPlatformConsts.MaxTraceIdLength);
        RemoteIpAddress = Check.Length(remoteIpAddress, nameof(remoteIpAddress), OpenPlatformConsts.MaxRemoteIpAddressLength);
        UserAgent = Check.Length(userAgent, nameof(userAgent), OpenPlatformConsts.MaxUserAgentLength);
        Timestamp = timestamp;
        ElapsedMs = elapsedMs;
        Succeeded = succeeded;
        FailureCode = Check.Length(failureCode, nameof(failureCode), OpenPlatformConsts.MaxFailureCodeLength);
        FailureMessage = Check.Length(failureMessage, nameof(failureMessage), OpenPlatformConsts.MaxFailureMessageLength);
        ResponseStatusCode = responseStatusCode;
    }
}
