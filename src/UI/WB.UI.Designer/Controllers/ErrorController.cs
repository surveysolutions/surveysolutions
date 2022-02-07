using Microsoft.AspNetCore.Mvc;

namespace WB.UI.Designer.Controllers
{
    [Route("error")]
    public class ErrorController : Controller
    {
        [Route("500")]
        public ActionResult Index() => View("Index");
        [Route("404")]
        public new ActionResult NotFound() => View("NotFound");
        [Route("401")]
        public ActionResult AccessDenied() => View("AccessDenied");
        [Route("403")]
        public ActionResult Forbidden() => this.View("Forbidden");
        
        [Route("{statusCode}")]

        public IActionResult Error(int? statusCode = null)
        {
            if (statusCode.HasValue)
            {
                switch (statusCode.Value)
                {
                    case 401: return AccessDenied();
                    case 403: return Forbidden();
                    case 404: return NotFound();
                    case 500: return Index();
                }
            }
            return Index();
        }
    }
}
