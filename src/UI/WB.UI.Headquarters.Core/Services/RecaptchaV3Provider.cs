using System;
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
        private const string WebInterviewPath = "/WebInterview";
        private const string WebInterviewExpectedAction = "start";
        private const string AccountLogOnPath = "/Account/LogOn";
        private const string AccountLogOnExpectedAction = "login";
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

            if (request.Path.StartsWithSegments(WebInterviewPath, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(validationResult.action, WebInterviewExpectedAction, StringComparison.Ordinal))
                return false;

            if (request.Path.StartsWithSegments(AccountLogOnPath, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(validationResult.action, AccountLogOnExpectedAction, StringComparison.Ordinal))
                return false;

            return validationResult.score >= captchaConfig.Value.RecaptchaV3MinimumScore;
        }
    }
}
