using CheMa.VNext.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace CheMa.VNext.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class VNextController : AbpControllerBase
{
    protected VNextController()
    {
        LocalizationResource = typeof(VNextResource);
    }
}
