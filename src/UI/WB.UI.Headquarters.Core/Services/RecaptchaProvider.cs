using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using reCAPTCHA.AspNetCore;
using WB.UI.Shared.Web.Captcha;

namespace WB.UI.Headquarters.Services
{
    public class RecaptchaProvider : ICaptchaProvider
    {
        private readonly IRecaptchaService recaptchaService;

        public RecaptchaProvider(IRecaptchaService  recaptchaService)
        {
            this.recaptchaService = recaptchaService;
        }

        public async Task<bool> IsCaptchaValid(HttpRequest request)
        {
            var validationResult = await  recaptchaService.Validate(request);
            return validationResult.success;
        }
    }
}
