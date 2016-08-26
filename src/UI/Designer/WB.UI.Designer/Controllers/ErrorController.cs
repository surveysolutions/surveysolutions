using System.Web.Mvc;

namespace WB.UI.Designer.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult NotFound()
        {
            return View();
        }

        public ActionResult AccessDenied()
        {
            return View();
        }

        public ActionResult Forbidden()
        {
            return this.View();
        }

        public ActionResult RequestLengthExceeded()
        {
            return this.View();

        }
    }
}
