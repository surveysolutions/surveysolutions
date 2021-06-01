using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace WB.UI.Shared.Web.Services
{
    public interface ICaptchaService
    {
        bool ShouldShowCaptcha(string username);
        void RegisterFailedLogin(string username);
        void ResetFailedLogin(string username);
    }

    public sealed class WebCacheBasedCaptchaService : ICaptchaService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IMemoryCache cache;
        private readonly IOptions<CaptchaConfig> captchaConfig;

        public WebCacheBasedCaptchaService(
            IHttpContextAccessor httpContextAccessor,
            IMemoryCache cache,
            IOptions<CaptchaConfig> captchaConfig)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.cache = cache;
            this.captchaConfig = captchaConfig;
        }

        private string GetCacheKey(string name) => $"_captcha_{name}";
        private const string ShouldShowCaptchaCookieName = "ShowCaptcha";

        public bool ShouldShowCaptcha(string username)
        {

            if (httpContextAccessor.HttpContext != null)
            {
                var requestCookieCollection = httpContextAccessor.HttpContext.Request.Cookies;
                if (requestCookieCollection.ContainsKey(ShouldShowCaptchaCookieName))
                {
                    string requestCookie = requestCookieCollection[ShouldShowCaptchaCookieName];
                    return !string.IsNullOrEmpty(requestCookie);
                }
            }

            if (string.IsNullOrEmpty(username)) return false;
            var userKey = this.GetCacheKey(username);

            Queue<DateTime> existingLoginAttempts = cache.Get<Queue<DateTime>>(userKey);

            if (existingLoginAttempts == null) return false;

            while (existingLoginAttempts.Count > 0 && DateTime.UtcNow - existingLoginAttempts.Peek() > SlidingWindow)
            {
                existingLoginAttempts.Dequeue();
            }

            if (existingLoginAttempts.Count == 0) return false;

            cache.Set(userKey,
                existingLoginAttempts,
                new MemoryCacheEntryOptions
                {
                    SlidingExpiration = SlidingWindow
                });

            return existingLoginAttempts.Count >= MaxFailedLoginAttemps;
        }

        public int MaxFailedLoginAttemps => this.captchaConfig.Value.CountOfFailedLoginAttemptsBeforeCaptcha;

        public TimeSpan SlidingWindow => TimeSpan.FromMinutes(this.captchaConfig.Value.TimespanInMinutesCaptchaWillBeShownAfterFailedLoginAttempt);

        public void RegisterFailedLogin(string username)
        {
            var userKey = this.GetCacheKey(username);

            var existingLoginAttempts =cache.Get<Queue<DateTime>>(userKey) ??
                new Queue<DateTime>(MaxFailedLoginAttemps + 1);

            existingLoginAttempts.Enqueue(DateTime.UtcNow);

            var invalidAttemptsExceededLimit = existingLoginAttempts.Count >= MaxFailedLoginAttemps;
            var expire = DateTime.UtcNow + SlidingWindow;

            if (invalidAttemptsExceededLimit)
            {
                httpContextAccessor.HttpContext?.Response.Cookies.Append(ShouldShowCaptchaCookieName, "true",
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Expires = expire
                    });
            }
            
            cache.Set(userKey,
                existingLoginAttempts,
                new MemoryCacheEntryOptions
                {
                    SlidingExpiration = SlidingWindow
                });
        }
        
        public void ResetFailedLogin(string username)
        {
            cache.Remove(GetCacheKey(username));

            httpContextAccessor.HttpContext?.Response.Cookies.Delete(ShouldShowCaptchaCookieName);
        }
    }
}
