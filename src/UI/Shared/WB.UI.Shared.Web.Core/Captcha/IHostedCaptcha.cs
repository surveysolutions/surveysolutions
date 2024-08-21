using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WB.UI.Shared.Web.Captcha
{
    public interface IHostedCaptcha
    {
        HtmlString Render();
    }
}
