using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [AllowAnonymous]
    [Route("error")]
    public class ErrorController : Controller
    {
        [Route("404")]
        public new IActionResult NotFound()
        {
            return View();
        }

        [Route("401")]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [Route("403")]
        public IActionResult Forbidden()
        {
            return this.View();
        }

        [Route("500")]
        public IActionResult Error()
        {
            return this.View();
        }
    }
}
