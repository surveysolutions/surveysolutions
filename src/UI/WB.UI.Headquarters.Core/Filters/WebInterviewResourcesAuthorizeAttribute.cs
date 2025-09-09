using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WB.UI.Headquarters.Filters
{
    public class WebInterviewResourcesAuthorizeAttribute : WebInterviewAuthorizeBaseAttribute
    {
        protected override bool IsReview(ActionExecutingContext context)
        {
            var ctx = context.HttpContext;
            var referer = ctx.Request.Headers.Referer;
            var url = referer.ToString();
            if (string.IsNullOrEmpty(url))
                return false;
            
            var uri = new Uri(url);
            var segments = uri.Segments;
            bool isReview = segments.Any(s => s.Trim('/').Equals("Review", StringComparison.OrdinalIgnoreCase));
            return isReview;
        }
    }
}
