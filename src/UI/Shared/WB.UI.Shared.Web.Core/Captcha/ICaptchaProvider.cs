using System.Web;
using System.Web.Mvc;

namespace WB.UI.Shared.Web.Captcha
{
    public interface ICaptchaProvider
    {
        IHtmlString RenderCaptcha(HtmlHelper helper);
        bool IsCaptchaValid(Controller controller);
    }
}