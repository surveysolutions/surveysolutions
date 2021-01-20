using Microsoft.AspNetCore.Mvc;
using WB.UI.Headquarters.Code.Workspaces;
using WB.UI.Shared.Web.Attributes;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [AllowDisabledWorkspaceAccess]
    [AllowPrimaryWorkspaceFallback]
    [NoTransaction]
    [Route("error")]
    public class ErrorController : Controller
    {
        [Route("404")]
        public new IActionResult NotFound() => View("NotFound");

        [Route("401")]
        public IActionResult AccessDenied() => View("AccessDenied");

        [Route("403")]
        public IActionResult Forbidden() => this.View("Forbidden");

        [Route("500")]
        public IActionResult UnhandledException() => this.View("UnhandledException");

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
                    case 500: return UnhandledException();
                }
            }
            return UnhandledException();
        }
    }
}
