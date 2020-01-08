using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WB.UI.Headquarters.Filters
{
    public class AntiForgeryFilter : ResultFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            IAntiforgery xsrf = (IAntiforgery) context.HttpContext.RequestServices.GetService(typeof(IAntiforgery));
            var token = xsrf.GetAndStoreTokens(context.HttpContext);
            context.HttpContext.Response.Cookies.Append("CSRF-TOKEN", token.RequestToken, 
                new Microsoft.AspNetCore.Http.CookieOptions { HttpOnly = false });
        }
    }
}
