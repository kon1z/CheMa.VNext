using System;
using Volo.Abp.Application.Dtos;

namespace CheMa.VNext.OpenPlatform.Dtos;

public class OpenAppDto : AuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string AppSecretMaskedHint { get; set; } = string.Empty;

    public OpenAppStatus Status { get; set; }

    public DateTime? BeginTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string? AllowedIpRanges { get; set; }

    public string? Description { get; set; }

    public DateTime? LastAccessTime { get; set; }

    public string? LastAccessIp { get; set; }
}
