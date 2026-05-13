using System;
using System.Threading;
using Volo.Abp;
using Volo.Abp.MultiTenancy;

namespace CheMa.VNext.MultiTenancy;

public class NullTenant : ICurrentTenant, ICurrentTenantAccessor
{
    private static readonly AsyncLocal<BasicTenantInfo?> CurrentTenant = new();

    public BasicTenantInfo? Current
    {
        get => CurrentTenant.Value ??= new BasicTenantInfo(null, null);
        set => CurrentTenant.Value = value ?? new BasicTenantInfo(null, null);
    }

    private BasicTenantInfo CurrentOrDefault => Current!;

    public bool IsAvailable => Id.HasValue;

    public Guid? Id => CurrentOrDefault.TenantId;

    public string? Name => CurrentOrDefault.Name;

    public IDisposable Change(Guid? id, string? name = null)
    {
        var parent = Current;
        Current = new BasicTenantInfo(id, name);

        return new DisposeAction(() => Current = parent);
    }
}
