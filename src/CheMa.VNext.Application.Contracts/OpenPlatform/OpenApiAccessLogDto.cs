using System;
using Volo.Abp.Application.Dtos;

namespace CheMa.VNext.OpenPlatform;

public class OpenApiAccessLogDto : EntityDto<Guid>
{
    public Guid? OpenAppId { get; set; }

    public string? ClientId { get; set; }

    public string RequestPath { get; set; } = string.Empty;

    public string HttpMethod { get; set; } = string.Empty;

    public string? QueryString { get; set; }

    public string? TraceId { get; set; }

    public string? RemoteIpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime Timestamp { get; set; }

    public long ElapsedMs { get; set; }

    public bool Succeeded { get; set; }

    public string? FailureCode { get; set; }

    public string? FailureMessage { get; set; }

    public int? ResponseStatusCode { get; set; }
}
