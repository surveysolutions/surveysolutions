using System.Web.Mvc;
using WB.UI.Shared.Web.Attributes;

namespace WB.UI.Designer.Controllers
{
    [AllowAnonymous]
    public class MaintenanceController : Controller
    {
        [NoTransaction]
        public ActionResult WaitForReadSideRebuild(string returnUrl)
        {
            return this.View(model: returnUrl);
        }

        [NoTransaction]
        public ActionResult ReadSideIsOutdated(string returnUrl)
        {
            return this.View(model: returnUrl);
        }
    }
}