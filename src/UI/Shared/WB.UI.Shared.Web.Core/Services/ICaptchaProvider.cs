using Markdig.Helpers;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace WB.UI.Shared.Web.Captcha
{
    public interface ICaptchaProvider
    {
        IHtmlContent RenderCaptcha(HtmlHelper helper);
        bool IsCaptchaValid(Controller controller);
    }
}
