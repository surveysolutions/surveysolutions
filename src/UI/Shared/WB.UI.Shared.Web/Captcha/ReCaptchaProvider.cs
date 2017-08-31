using System.Globalization;
using System.Web;
using System.Web.Mvc;
using Recaptcha.Web;
using Recaptcha.Web.Mvc;
using WB.UI.Shared.Web.Configuration;

namespace WB.UI.Shared.Web.Captcha
{
    public class ReCaptchaProvider : ICaptchaProvider
    {
        private readonly IConfigurationManager configurationManager;

        public ReCaptchaProvider(IConfigurationManager configurationManager)
        {
            this.configurationManager = configurationManager;
        }

        public IHtmlString RenderCaptcha(HtmlHelper helper) => helper.Recaptcha(language: CultureInfo.CurrentCulture.TwoLetterISOLanguageName);

        public bool IsCaptchaValid(Controller controller)
        {
            var verificationHelper = controller.GetRecaptchaVerificationHelper();
            var response = verificationHelper.VerifyRecaptchaResponse();
            return response == RecaptchaVerificationResult.Success;
        }
    }
}