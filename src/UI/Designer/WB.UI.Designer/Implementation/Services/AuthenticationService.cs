using System;
using System.Web;
using WB.UI.Designer.Services;
using WB.UI.Shared.Web.Captcha;
using WB.UI.Shared.Web.Configuration;
using WebMatrix.WebData;

namespace WB.UI.Designer.Implementation.Services
{
    internal class AuthenticationService : IAuthenticationService
    {
        private readonly ICaptchaService captchaService;
        private readonly IConfigurationManager configurationManager;

        public AuthenticationService(ICaptchaService captchaService, IConfigurationManager configurationManager)
        {
            this.captchaService = captchaService;
            this.configurationManager = configurationManager;
        }

        public bool Login(string userName, string password, bool staySignedIn)
        {
            if (!WebSecurity.Login(userName, password, staySignedIn))
            {
                if (IsCaptchaEnabled)
                {
                    captchaService.RegisterFailedLogin(userName);
                }
                return false;
            }

            if (staySignedIn)
            {
                HttpContext.Current.Response.Cookies[0].Expires = DateTime.Now.AddDays(1);
            }

            captchaService.ResetFailedLogin(userName);

            return true;
        }

        private bool IsCaptchaEnabled => AppSettings.Instance.IsReCaptchaEnabled 
            && !string.IsNullOrEmpty(this.configurationManager.AppSettings["ReCaptchaPrivateKey"]) 
            && !string.IsNullOrEmpty(this.configurationManager.AppSettings["ReCaptchaPublicKey"]);

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