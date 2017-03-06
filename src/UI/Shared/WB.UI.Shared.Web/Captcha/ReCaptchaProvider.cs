using System.Web;
using System.Web.Mvc;
using Recaptcha.Web;
using Recaptcha.Web.Mvc;

namespace WB.UI.Shared.Web.Captcha
{
    public class ReCaptchaProvider : ICaptchaProvider
    {
        public IHtmlString RenderCaptcha(HtmlHelper helper) => helper.Recaptcha();

        public bool IsCaptchaValid(Controller controller)
        {
            var verificationHelper = controller.GetRecaptchaVerificationHelper();
            var response = verificationHelper.VerifyRecaptchaResponse();
            return response == RecaptchaVerificationResult.Success;
        }
    }
}