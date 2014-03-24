using System.Web.Mvc;

namespace WB.UI.Headquarters.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        public ActionResult Index()
        {
            return this.View();
        }

        public ActionResult NotFound()
        {
            return this.View();
        }

        public ActionResult AccessDenied()
        {
            return this.View();
        }

        public ActionResult Forbidden()
        {
            return this.View();
        }
    }
}
