using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using reCAPTCHA.AspNetCore;
using WB.UI.Shared.Web.Captcha;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Headquarters.Services
{
    public class RecaptchaV3Provider : ICaptchaProvider
    {
        private readonly IRecaptchaService recaptchaService;
        private readonly IOptions<CaptchaConfig> captchaConfig;

        public RecaptchaV3Provider(IRecaptchaService recaptchaService, IOptions<CaptchaConfig> captchaConfig)
        {
            this.recaptchaService = recaptchaService;
            this.captchaConfig = captchaConfig;
        }

        public async Task<bool> IsCaptchaValid(HttpRequest request)
        {
            var validationResult = await recaptchaService.Validate(request);
            if (!validationResult.success)
                return false;

            return validationResult.score >= captchaConfig.Value.RecaptchaV3MinimumScore;
        }
    }
}
