using System;
using System.Linq;
using System.Threading.Tasks;
using CheMa.VNext.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace CheMa.VNext.OpenPlatform;

public class OpenAppAppService : VNextAppService, IOpenAppAppService
{
    private readonly IRepository<OpenApp, Guid> _openAppRepository;
    private readonly OpenAppManager _openAppManager;

    public OpenAppAppService(
        IRepository<OpenApp, Guid> openAppRepository,
        OpenAppManager openAppManager)
    {
        _openAppRepository = openAppRepository;
        _openAppManager = openAppManager;
    }

    public async Task<OpenAppDto> GetAsync(Guid id)
    {
        await CheckGetPolicyAsync();
        var entity = await _openAppRepository.GetAsync(id);
        return MapToOpenAppDto(entity);
    }

    public async Task<PagedResultDto<OpenAppDto>> GetListAsync(GetOpenAppListInput input)
    {
        await CheckGetPolicyAsync();

        var queryable = await _openAppRepository.GetQueryableAsync();

        if (!input.Filter.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(x => x.Name.Contains(input.Filter!) || x.ClientId.Contains(input.Filter!));
        }

        if (input.Status.HasValue)
        {
            queryable = queryable.Where(x => x.Status == input.Status.Value);
        }

        var totalCount = queryable.LongCount();
        var items = queryable
            .OrderByDescending(x => x.CreationTime)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        return new PagedResultDto<OpenAppDto>(totalCount, items.Select(MapToOpenAppDto).ToList());
    }

    public async Task<OpenAppSecretResultDto> CreateAsync(CreateOpenAppDto input)
    {
        await CheckCreatePolicyAsync();
        var secretInfo = await _openAppManager.CreateAsync(input.Name, input.BeginTime, input.EndTime, input.AllowedIpRanges, input.Description);

        return new OpenAppSecretResultDto
        {
            OpenApp = MapToOpenAppDto(secretInfo.OpenApp),
            AppSecret = secretInfo.PlainSecret
        };
    }

    public async Task<OpenAppDto> UpdateAsync(Guid id, UpdateOpenAppDto input)
    {
        await CheckUpdatePolicyAsync();
        var entity = await _openAppRepository.GetAsync(id);

        entity.SetName(input.Name);
        entity.SetValidityPeriod(input.BeginTime, input.EndTime);
        entity.SetAllowedIpRanges(input.AllowedIpRanges);
        entity.SetDescription(input.Description);

        await _openAppRepository.UpdateAsync(entity, autoSave: true);
        return MapToOpenAppDto(entity);
    }

    public async Task EnableAsync(Guid id)
    {
        await CheckEnablePolicyAsync();
        var entity = await _openAppRepository.GetAsync(id);
        entity.Enable();
        await _openAppRepository.UpdateAsync(entity, autoSave: true);
    }

    public async Task DisableAsync(Guid id)
    {
        await CheckDisablePolicyAsync();
        var entity = await _openAppRepository.GetAsync(id);
        entity.Disable();
        await _openAppRepository.UpdateAsync(entity, autoSave: true);
    }

    public async Task<OpenAppSecretResultDto> ResetSecretAsync(Guid id)
    {
        await CheckResetSecretPolicyAsync();
        var entity = await _openAppRepository.GetAsync(id);
        var secretInfo = await _openAppManager.ResetSecretAsync(entity);

        return new OpenAppSecretResultDto
        {
            OpenApp = MapToOpenAppDto(secretInfo.OpenApp),
            AppSecret = secretInfo.PlainSecret
        };
    }

    protected virtual Task CheckGetPolicyAsync()
    {
        return Task.CompletedTask;
    }

    protected virtual Task CheckCreatePolicyAsync()
    {
        return Task.CompletedTask;
    }

    protected virtual Task CheckUpdatePolicyAsync()
    {
        return Task.CompletedTask;
    }

    protected virtual Task CheckEnablePolicyAsync()
    {
        return Task.CompletedTask;
    }

    protected virtual Task CheckDisablePolicyAsync()
    {
        return Task.CompletedTask;
    }

    protected virtual Task CheckResetSecretPolicyAsync()
    {
        return Task.CompletedTask;
    }

    private static OpenAppDto MapToOpenAppDto(OpenApp entity)
    {
        return new OpenAppDto
        {
            Id = entity.Id,
            Name = entity.Name,
            ClientId = entity.ClientId,
            AppSecretMaskedHint = entity.AppSecretMaskedHint,
            Status = entity.Status,
            BeginTime = entity.BeginTime,
            EndTime = entity.EndTime,
            AllowedIpRanges = entity.AllowedIpRanges,
            Description = entity.Description,
            LastAccessTime = entity.LastAccessTime,
            LastAccessIp = entity.LastAccessIp,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId
        };
    }
}
