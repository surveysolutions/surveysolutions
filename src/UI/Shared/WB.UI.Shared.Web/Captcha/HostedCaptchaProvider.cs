using System.Web;
using System.Web.Mvc;
using CaptchaMvc.HtmlHelpers;

namespace WB.UI.Shared.Web.Captcha
{
    public class HostedCaptchaProvider : ICaptchaProvider
    {
        public IHtmlString RenderCaptcha(HtmlHelper helper) => helper.Captcha(Resources.Captcha.TryAnother, Resources.Captcha.EnterText, 5, Resources.Captcha.Required, true);

        public bool IsCaptchaValid(Controller controller) => controller.IsCaptchaValid(Resources.Captcha.InvalidCaptcha);
    }
}