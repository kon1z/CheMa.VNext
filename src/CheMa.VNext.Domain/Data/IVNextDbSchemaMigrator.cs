using System.Threading.Tasks;

namespace CheMa.VNext.Data;

public interface IVNextDbSchemaMigrator
{
    Task MigrateAsync();
}
