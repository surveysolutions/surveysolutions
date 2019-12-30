using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WB.UI.Shared.Web.Captcha
{
    public static class CaptchaExtensions
    {
        public static IConfigurationSection CaptchaOptionsSection(this IConfiguration configuration)
        {
            return configuration.GetSection("Captcha");
        }
    }
}
