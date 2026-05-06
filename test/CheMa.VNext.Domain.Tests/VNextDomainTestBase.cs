using Volo.Abp.Modularity;

namespace CheMa.VNext;

/* Inherit from this class for your domain layer tests. */
public abstract class VNextDomainTestBase<TStartupModule> : VNextTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
