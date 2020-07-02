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
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            
        }
    }
}