using System;
using System.Web;
using System.Web.Mvc;
using CaptchaMvc.HtmlHelpers;

namespace WB.UI.Shared.Web.Captcha
{
    public class HostedCaptchaProvider : ICaptchaProvider
    {
        private const string BrTag = "<br/>";

        public IHtmlString RenderCaptcha(HtmlHelper helper)
        {
            var captchaMarkup = helper.Captcha(Resources.Captcha.TryAnother, Resources.Captcha.EnterText, 5,
                Resources.Captcha.Required, true).ToHtmlString();

            // there is no way to exclude those <br /> tags, so I will remove them(only first two)
            captchaMarkup = RemoveFirstOccurence(captchaMarkup, BrTag);
            captchaMarkup = RemoveFirstOccurence(captchaMarkup, BrTag);

            return new MvcHtmlString(captchaMarkup);
        }

        public bool IsCaptchaValid(Controller controller) => controller.IsCaptchaValid(Resources.Captcha.InvalidCaptcha);

        private string RemoveFirstOccurence(string source, string pattern)
        {
            var index = source.IndexOf(pattern, StringComparison.Ordinal);
            return index >= 0 ? source.Substring(0, index) + source.Substring(index + pattern.Length) : source;
        }
    }
}