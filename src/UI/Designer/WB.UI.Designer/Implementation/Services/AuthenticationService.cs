using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;
using WB.UI.Designer.Services;
using WB.UI.Shared.Web.Configuration;
using WebMatrix.WebData;

namespace WB.UI.Designer.Implementation.Services
{
    internal class AuthenticationService : IAuthenticationService
    {
        private readonly IConfigurationManager configurationManager;
        public AuthenticationService(IConfigurationManager configurationManager)
        {
            this.configurationManager = configurationManager;
        }

        private string ShouldShowCaptchaCookiesKey = "ShouldShowCaptcha";
        private void StoreInvalidAttempt(string userName)
        {
            if (this.IsCaptchaDisabled) return;

            var appSettings = AppSettings.Instance;
            Queue<DateTime> existingLoginAttempts = HttpContext.Current.Cache.Get(userName) as Queue<DateTime>;
            if (existingLoginAttempts == null)
            {
                existingLoginAttempts = new Queue<DateTime>(appSettings.CountOfFailedLoginAttemptsBeforeCaptcha + 1);
            }

            existingLoginAttempts.Enqueue(DateTime.Now);

            var invalidAttemptsExceededLimit = existingLoginAttempts.Count >= appSettings.CountOfFailedLoginAttemptsBeforeCaptcha;
            var expire = DateTime.Now + appSettings.TimespanInMinutesCaptchaWillBeShownAfterFailedLoginAttempt;

            if (invalidAttemptsExceededLimit)
            {
                HttpContext.Current.Response.Cookies.Add(new HttpCookie(this.ShouldShowCaptchaCookiesKey, invalidAttemptsExceededLimit.ToString())
                {
                    Expires = expire
                });
            }

            HttpContext.Current.Cache.Insert(userName,
                existingLoginAttempts,
                null,
                expire,
                Cache.NoSlidingExpiration);
        }

        private bool IsCaptchaDisabled => !AppSettings.Instance.IsReCaptchaEnabled ||
                                          string.IsNullOrEmpty(this.configurationManager.AppSettings["ReCaptchaPrivateKey"]) ||
                                          string.IsNullOrEmpty(this.configurationManager.AppSettings["ReCaptchaPublicKey"]);

        private void ResetInvalidAttemptsCount(string userName)
        {
            if (this.IsCaptchaDisabled) return;

            HttpContext.Current.Cache.Remove(userName);
            if (HttpContext.Current.Request.Cookies[this.ShouldShowCaptchaCookiesKey] != null)
            {
                HttpContext.Current.Request.Cookies.Remove(this.ShouldShowCaptchaCookiesKey);
                HttpContext.Current.Response.Cookies.Add(new HttpCookie(this.ShouldShowCaptchaCookiesKey) { Expires = DateTime.Now.AddDays(-1) });
            }
        }

        public bool Login(string userName, string password, bool staySignedIn)
        {
            if (!WebSecurity.Login(userName, password, staySignedIn))
            {
                this.StoreInvalidAttempt(userName);
                return false;
            }

            if (staySignedIn)
            {
                HttpContext.Current.Response.Cookies[0].Expires = DateTime.Now.AddDays(1);
            }

            this.ResetInvalidAttemptsCount(userName);

            return true;
        }

        public bool ShouldShowCaptcha()
        {
            var shouldShowCaptchaCookies = HttpContext.Current.Request.Cookies.Get(this.ShouldShowCaptchaCookiesKey);

            return !string.IsNullOrEmpty(shouldShowCaptchaCookies?.Value);
        }

        public bool ShouldShowCaptchaByUserName(string userName)
        {
            if (string.IsNullOrEmpty(userName)) return false;

            Queue<DateTime> existingLoginAttempts = HttpContext.Current.Cache.Get(userName) as Queue<DateTime>;

            return existingLoginAttempts?.Count >= AppSettings.Instance.CountOfFailedLoginAttemptsBeforeCaptcha;
        }
    }
}