using CheMa.VNext.Localization;
using Volo.Abp.Application.Services;

namespace CheMa.VNext.Base;

/* Inherit your application services from this class.
 */
public abstract class VNextAppService : ApplicationService
{
    protected VNextAppService()
    {
        LocalizationResource = typeof(VNextResource);
    }
}
