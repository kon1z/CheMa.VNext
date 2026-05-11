using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace CheMa.VNext.Controllers;

public class HomeController : AbpController
{
    public ActionResult Index() => Redirect("~/swagger");
}
