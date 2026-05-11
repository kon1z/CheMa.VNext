using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace CheMa.VNext.Data;

/* This is used if database provider does't define
 * IVNextDbSchemaMigrator implementation.
 */
public class NullVNextDbSchemaMigrator : IVNextDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync() => Task.CompletedTask;
}
