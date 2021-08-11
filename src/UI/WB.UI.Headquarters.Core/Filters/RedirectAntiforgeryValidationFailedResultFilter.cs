using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WB.UI.Headquarters.Filters
{
    public class RedirectAntiforgeryValidationFailedResultFilter : IAlwaysRunResultFilter
    {
        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is IAntiforgeryValidationFailedResult result)
            {
                var referer = context.HttpContext.Request.GetTypedHeaders().Referer.ToString();
                if (!string.IsNullOrEmpty(referer))
                {
                    //add validation against valid referer list
                    context.Result = new RedirectResult(referer);
                }
                else
                    context.Result = new RedirectToActionResult("AntiForgery","Error", null);
            }
        }

        public void OnResultExecuted(ResultExecutedContext context)
        { }
    }
}
