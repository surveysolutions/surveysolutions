using System.Web;
using System.Web.Mvc;

namespace WB.UI.Shared.Web.Captcha
{
    public class NoCaptchaProvider : ICaptchaProvider
    {
        public IHtmlString RenderCaptcha(HtmlHelper helper) => new MvcHtmlString(string.Empty);

        public bool IsCaptchaValid(Controller controller) => true;
    }
}
