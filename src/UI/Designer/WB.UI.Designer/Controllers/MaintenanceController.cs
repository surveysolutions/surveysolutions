namespace WB.UI.Designer.Controllers
{
    using System.Web.Mvc;

    public class MaintenanceController : Controller
    {
        public ActionResult ReadLayer(string returnUrl)
        {
            return this.View(model: returnUrl);
        }
    }
}