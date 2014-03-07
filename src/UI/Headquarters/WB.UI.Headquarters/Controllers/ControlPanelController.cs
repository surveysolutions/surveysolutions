using System.Web.Mvc;

namespace WB.UI.Headquarters.Controllers
{
    [AllowAnonymous]
    public class ControlPanelController : Controller
    {
        public ActionResult NConfig()
        {
            return this.View();
        }
    }
}