using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;
using WB.UI.Shared.Web.Configuration;

namespace WB.UI.Shared.Web.Captcha
{
    public sealed class WebCacheBasedCaptchaService : ICaptchaService
    {
        private readonly IConfigurationManager configurationManager;
        
        public WebCacheBasedCaptchaService(IConfigurationManager configurationManager)
        {
            this.configurationManager = configurationManager;
        }

        private int MaxFailedLoginAttemps => this.configurationManager.GetMaxFailedLoginCountBeforeCaptchaAppear();

        private TimeSpan SlidingWindow => this.configurationManager.TimespanInMinutesCaptchaWillBeShownAfterFailedLoginAttempt();
        
        private string GetCacheKey(string name) => $"_captcha_{name}";
        private const string ShouldShowCaptchaCookieName = "ShowCaptcha";

        public bool ShouldShowCaptcha(string username)
        {
            if (HttpContext.Current != null)
            {
                var shouldShowCaptchaCookies = HttpContext.Current.Request.Cookies.Get(ShouldShowCaptchaCookieName);

                if (!string.IsNullOrEmpty(shouldShowCaptchaCookies?.Value))
                {
                    return true;
                }
            }

            if (string.IsNullOrEmpty(username)) return false;
            var userKey = this.GetCacheKey(username);

            var existingLoginAttempts = Cache.Get(userKey) as Queue<DateTime>;

            if (existingLoginAttempts == null) return false;

            while (existingLoginAttempts.Count > 0 && (DateTime.UtcNow - existingLoginAttempts.Peek() > SlidingWindow))
            {
                existingLoginAttempts.Dequeue();
            }

            if (existingLoginAttempts.Count == 0) return false;

            Cache.Insert(userKey,
                existingLoginAttempts,
                null,
                Cache.NoAbsoluteExpiration,
                SlidingWindow);

            return existingLoginAttempts?.Count >= MaxFailedLoginAttemps;
        }

        public void RegisterFailedLogin(string username)
        {
            var userKey = this.GetCacheKey(username);

            var existingLoginAttempts = Cache.Get(userKey) as Queue<DateTime> ??
                new Queue<DateTime>(MaxFailedLoginAttemps + 1);

            existingLoginAttempts.Enqueue(DateTime.UtcNow);

            var invalidAttemptsExceededLimit = existingLoginAttempts.Count >= MaxFailedLoginAttemps;
            var expire = DateTime.UtcNow + SlidingWindow;

            if (invalidAttemptsExceededLimit && HttpContext.Current != null)
            {
                HttpContext.Current.Response.Cookies.Add(new HttpCookie(ShouldShowCaptchaCookieName, invalidAttemptsExceededLimit.ToString())
                {
                    Expires = expire
                });
            }
            
            Cache.Insert(userKey,
                existingLoginAttempts,
                null,
                Cache.NoAbsoluteExpiration,
                SlidingWindow);
        }
        
        public void ResetFailedLogin(string username)
        {
            Cache.Remove(GetCacheKey(username));

            if (HttpContext.Current != null)
            {
                HttpContext.Current.Response.SetCookie(new HttpCookie(ShouldShowCaptchaCookieName)
                {
                    Value = string.Empty,
                    Expires = DateTime.MinValue
                });
            }
        }

        private static Cache Cache => HttpContext.Current == null
          ? HttpRuntime.Cache
          : HttpContext.Current.Cache;
    }
}