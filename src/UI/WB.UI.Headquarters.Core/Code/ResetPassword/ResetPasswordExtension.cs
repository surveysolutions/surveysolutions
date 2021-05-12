using Microsoft.AspNetCore.Builder;

namespace WB.UI.Headquarters.Code.ResetPassword
{
    public static class ResetPasswordExtension
    {
        public static void UseResetPasswordRedirect(this IApplicationBuilder app)
        {
            app.UseMiddleware<ResetPasswordMiddleware>();
        }

    }
}