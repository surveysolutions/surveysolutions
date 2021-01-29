using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.UI.Headquarters.Services.Impl;

namespace WB.UI.Headquarters.Code.ResetPassword
{
    public class ResetPasswordMiddleware
    {
        private readonly RequestDelegate next;
        
        public ResetPasswordMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public Task Invoke(HttpContext context)
        {
            var authorizedUser = context.RequestServices.GetRequiredService<IAuthorizedUser>();

            if (authorizedUser != null && authorizedUser.NeedChangePassword)
            {
                if (!context.Request.Path.StartsWithSegments("/Users/ChangePassword")
                    && !context.Request.Path.StartsWithSegments("/Users/UpdatePassword")
                    && !context.Request.Path.StartsWithSegments("/.hc")
                    && !context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.Redirect("/Users/ChangePassword");
                    return Task.CompletedTask;
                }
            }

            return next.Invoke(context);
        }
    }
}