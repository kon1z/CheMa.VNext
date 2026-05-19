using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace CheMa.VNext.OpenPlatform.Entities;

public class OpenApp : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;

    public string ClientId { get; private set; } = string.Empty;

    public string AppSecretCipherText { get; private set; } = string.Empty;

    public string AppSecretMaskedHint { get; private set; } = string.Empty;

    public OpenAppStatus Status { get; private set; }

    public DateTime? BeginTime { get; private set; }

    public DateTime? EndTime { get; private set; }

    public string? AllowedIpRanges { get; private set; }

    public string? Description { get; private set; }

    public DateTime? LastAccessTime { get; private set; }

    public string? LastAccessIp { get; private set; }

    protected OpenApp()
    {
    }

    public OpenApp(
        Guid id,
        string name,
        string clientId,
        string appSecretCipherText,
        string appSecretMaskedHint,
        DateTime? beginTime = null,
        DateTime? endTime = null,
        string? allowedIpRanges = null,
        string? description = null)
        : base(id)
    {
        SetName(name);
        SetClientId(clientId);
        SetSecret(appSecretCipherText, appSecretMaskedHint);
        SetValidityPeriod(beginTime, endTime);
        SetAllowedIpRanges(allowedIpRanges);
        SetDescription(description);
        Status = OpenAppStatus.Enabled;
    }

    public void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), OpenPlatformConsts.MaxNameLength);
    }

    public void SetSecret(string appSecretCipherText, string appSecretMaskedHint)
    {
        AppSecretCipherText = Check.NotNullOrWhiteSpace(appSecretCipherText, nameof(appSecretCipherText), OpenPlatformConsts.MaxSecretCipherTextLength);
        AppSecretMaskedHint = Check.NotNullOrWhiteSpace(appSecretMaskedHint, nameof(appSecretMaskedHint), OpenPlatformConsts.MaxSecretMaskedHintLength);
    }

    public void SetValidityPeriod(DateTime? beginTime, DateTime? endTime)
    {
        if (beginTime.HasValue && endTime.HasValue && beginTime.Value > endTime.Value)
        {
            throw new BusinessException("VNext:OpenPlatform:InvalidValidityPeriod")
                .WithData(nameof(beginTime), beginTime)
                .WithData(nameof(endTime), endTime);
        }

        BeginTime = beginTime;
        EndTime = endTime;
    }

    public void SetAllowedIpRanges(string? allowedIpRanges)
    {
        AllowedIpRanges = Check.Length(allowedIpRanges, nameof(allowedIpRanges), OpenPlatformConsts.MaxIpRangesLength);
    }

    public void SetDescription(string? description)
    {
        Description = Check.Length(description, nameof(description), OpenPlatformConsts.MaxDescriptionLength);
    }

    public void Enable()
    {
        Status = OpenAppStatus.Enabled;
    }

    public void Disable()
    {
        Status = OpenAppStatus.Disabled;
    }

    public void UpdateLastAccess(DateTime accessTime, string? ipAddress)
    {
        LastAccessTime = accessTime;
        LastAccessIp = Check.Length(ipAddress, nameof(ipAddress), OpenPlatformConsts.MaxRemoteIpAddressLength);
    }

    public bool IsAvailable(DateTime now)
    {
        if (Status != OpenAppStatus.Enabled)
        {
            return false;
        }

        if (BeginTime.HasValue && now < BeginTime.Value)
        {
            return false;
        }

        if (EndTime.HasValue && now > EndTime.Value)
        {
            return false;
        }

        return true;
    }

    private void SetClientId(string clientId)
    {
        ClientId = Check.NotNullOrWhiteSpace(clientId, nameof(clientId), OpenPlatformConsts.MaxClientIdLength);
    }
}
