using System.Web.Mvc;

namespace WB.UI.Designer.Controllers
{
    public class MaintenanceController : Controller
    {
        public ActionResult WaitForReadLayerRebuild(string returnUrl)
        {
            return this.View(model: returnUrl);
        }
    }
}