using System.Web.Mvc;
using WB.UI.Shared.Web.Attributes;

namespace WB.UI.Designer.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        [NoTransaction]
        public ActionResult Index()
        {
            return View();
        }

        [NoTransaction]
        public ActionResult NotFound()
        {
            return View();
        }

        [NoTransaction]
        public ActionResult AccessDenied()
        {
            return View();
        }

        [NoTransaction]
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
