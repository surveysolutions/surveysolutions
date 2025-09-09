using Microsoft.AspNetCore.Mvc.Filters;

namespace WB.UI.Headquarters.Filters
{
    public class WebInterviewAuthorizeAttribute : WebInterviewAuthorizeBaseAttribute
    {
        protected override bool IsReview(ActionExecutingContext context)
        {
            var ctx = context.HttpContext;
            var isReview = ctx.Request.Headers.ContainsKey(@"review");
            return isReview;
        }
    }
}
