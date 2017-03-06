using System.Web;
using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;

namespace WB.UI.Shared.Web.Captcha
{
    public static class ViewHelper
    {
        public static IHtmlString RenderCaptcha(this HtmlHelper helper)
        {
            var provider = ServiceLocator.Current.GetInstance<ICaptchaProvider>();
            return provider?.RenderCaptcha(helper) ?? new MvcHtmlString(string.Empty);
        }
    }
}