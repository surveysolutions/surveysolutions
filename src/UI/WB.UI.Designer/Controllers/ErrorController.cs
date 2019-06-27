using Microsoft.AspNetCore.Mvc;

namespace WB.UI.Designer.Controllers
{
    [Route("error")]
    public class ErrorController : Controller
    {
        [Route("500")]
        public ActionResult Index() => View();
        [Route("404")]
        public new ActionResult NotFound() => View();
        [Route("401")]
        public ActionResult AccessDenied() => View();
        [Route("403")]
        public ActionResult Forbidden() => this.View();
    }
}
