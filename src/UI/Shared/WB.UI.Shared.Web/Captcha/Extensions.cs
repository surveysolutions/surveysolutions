using System;
using System.Web;
using System.Web.Mvc;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.UI.Shared.Web.Configuration;
using WB.UI.Shared.Web.Extensions;

namespace WB.UI.Shared.Web.Captcha
{
    public static class Extensions
    {
        public static IHtmlString RenderCaptcha(this HtmlHelper helper, ICaptchaProvider captchaProvider)
        {
            return captchaProvider?.RenderCaptcha(helper) ?? new MvcHtmlString(string.Empty);
        }

        public static string GetCaptchaService(this IConfigurationManager manager)
        {
            return manager.AppSettings.GetString("CaptchaService", "hosted");
        }

        public static int GetMaxFailedLoginCountBeforeCaptchaAppear(this IConfigurationManager manager)
        {
            return manager.AppSettings.GetInt("CountOfFailedLoginAttemptsBeforeCaptcha", 5);
        }

        public static TimeSpan TimespanInMinutesCaptchaWillBeShownAfterFailedLoginAttempt(this IConfigurationManager manager)
        {
             return TimeSpan.FromMinutes(manager.AppSettings.GetInt("TimespanInMinutesCaptchaWillBeShownAfterFailedLoginAttempt", 5)); 
        }
    }
}
