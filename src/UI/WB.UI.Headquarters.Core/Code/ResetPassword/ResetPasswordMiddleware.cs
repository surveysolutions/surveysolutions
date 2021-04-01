using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using WB.Core.BoundedContexts.Headquarters.Services;

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

            if (authorizedUser != null && authorizedUser.PasswordChangeRequired)
            {
                if (!context.Request.Path.StartsWithSegments("/ChangePassword")
                    && !context.Request.Path.StartsWithSegments("/Users/UpdatePassword")
                    && !context.Request.Path.StartsWithSegments("/Account/LogOff")
                    && !context.Request.Path.StartsWithSegments("/.hc")
                    && !context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.Redirect("/users/ChangePassword");
                    return Task.CompletedTask;
                }
            }

            return next.Invoke(context);
        }
    }
}
