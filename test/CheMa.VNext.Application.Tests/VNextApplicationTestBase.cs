using Volo.Abp.Modularity;

namespace CheMa.VNext;

public abstract class VNextApplicationTestBase<TStartupModule> : VNextTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
