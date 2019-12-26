using Microsoft.Extensions.DependencyInjection;

namespace WB.UI.Shared.Web.Captcha
{
    public static class CaptchaExtensions
    {
        public static void UseHostedCaptcha(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<ICaptchaProvider, HostedCaptchaProvider>();
            serviceCollection.AddTransient<IHostedCaptcha, HostedCaptchaProvider>();
        }
    }
}