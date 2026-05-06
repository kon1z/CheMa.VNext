using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CheMa.VNext.Data;
using Volo.Abp.DependencyInjection;

namespace CheMa.VNext.EntityFrameworkCore;

public class EntityFrameworkCoreVNextDbSchemaMigrator
    : IVNextDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreVNextDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolve the VNextDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<VNextDbContext>()
            .Database
            .MigrateAsync();
    }
}
