using Microsoft.AspNetCore.Mvc;

namespace WB.UI.Designer.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Index() => View();

        public ActionResult NotFound() => View();

        public ActionResult AccessDenied() => View();

        public ActionResult Forbidden() => this.View();
    }
}
