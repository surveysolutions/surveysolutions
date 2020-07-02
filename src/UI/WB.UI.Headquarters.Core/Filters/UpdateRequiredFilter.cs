using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using WB.Core.Infrastructure.Versions;

namespace WB.UI.Headquarters.Filters
{
    public class UpdateRequiredFilter : IActionFilter
    {
        private readonly IProductVersion productVersion;

        public UpdateRequiredFilter(IProductVersion productVersion)
        {
            this.productVersion = productVersion;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Result is IStatusCodeActionResult statusCodeResult
                && statusCodeResult.StatusCode == StatusCodes.Status426UpgradeRequired)
            {
                var version = productVersion.ToString();
                context.HttpContext.Response.Headers.Add("version", version);
                
                /*if (context.Result is JsonResult jsonResult && jsonResult.Value != null)
                    return;
                if (context.Result is ObjectResult objectResult && objectResult.Value != null)
                    return;
                
                var version = productVersion.ToString();
                var versionInfo = new ProductVersionInfo()
                {
                    Version = version
                };
                context.Result = new JsonResult(versionInfo)
                {
                    StatusCode = StatusCodes.Status426UpgradeRequired
                };*/
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            
        }
    }
}