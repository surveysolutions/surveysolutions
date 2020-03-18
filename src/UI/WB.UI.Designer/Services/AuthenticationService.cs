using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.UI.Designer.CommonWeb;
using WB.UI.Designer.Models;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Designer.Implementation.Services
{
    public interface ICaptchaProtectedAuthenticationService
    {
        bool ShouldShowCaptchaByUserName(string userName);
        bool ShouldShowCaptcha();
        Task<bool> Login(string userName, string password, bool staySignedIn);
    }

    internal class CaptchaProtectedAuthenticationService : ICaptchaProtectedAuthenticationService
    {
        private readonly ICaptchaService captchaService;
        private readonly IOptions<CaptchaConfig> captchaOptions;
        private readonly SignInManager<DesignerIdentityUser> signInManager;

        public CaptchaProtectedAuthenticationService(ICaptchaService captchaService,
            IOptions<CaptchaConfig> captchaOptions,
            SignInManager<DesignerIdentityUser> signInManager
            )
        {
            this.captchaService = captchaService;
            this.captchaOptions = captchaOptions;
            this.signInManager = signInManager;
        }

        public async Task<bool> Login(string userName, string password, bool staySignedIn)
        {
            if (await this.signInManager.PasswordSignInAsync(userName, password, staySignedIn, false) != SignInResult.Success)
            {
                if (IsCaptchaEnabled)
                {
                    captchaService.RegisterFailedLogin(userName);
                }
                return false;
            }

            captchaService.ResetFailedLogin(userName);

            return true;
        }

        private bool IsCaptchaEnabled => this.captchaOptions.Value.CaptchaType == CaptchaProviderType.Recaptcha;

        public bool ShouldShowCaptcha()
        {
            return this.captchaService.ShouldShowCaptcha(null);
        }

        public bool ShouldShowCaptchaByUserName(string userName)
        {
            return this.captchaService.ShouldShowCaptcha(userName);
        }
    }
}
