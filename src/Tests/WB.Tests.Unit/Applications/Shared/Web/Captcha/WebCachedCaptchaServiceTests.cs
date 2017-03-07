using System;
using System.Collections.Specialized;
using Machine.Specifications;
using NUnit.Framework;
using WB.Tests.Abc;
using WB.UI.Shared.Web.Captcha;

namespace WB.Tests.Unit.Applications.Shared.Web.Captcha
{
    public class WebCachedCaptchaServiceTests
    {
        private WebCacheBasedCaptchaService Subject;
        private const int NumberOfAttempts = 5;

        [SetUp]
        public void Setup()
        {
            var configurator = Create.Service.ConfigurationManager(new NameValueCollection
            {
                {"CountOfFailedLoginAttemptsBeforeCaptcha", NumberOfAttempts.ToString()},
                {"TimespanInMinutesCaptchaWillBeShownAfterFailedLoginAttempt", "5"},
            });

            this.Subject = new WebCacheBasedCaptchaService(configurator);
        }

        [Test]
        public void ShouldNotRequireCaptchaForFirst5Attempts()
        {
            var userName = Guid.NewGuid().ToString();

            this.Subject.ShouldShowCaptcha(userName).ShouldBeFalse();

            for (int i = 0; i < NumberOfAttempts - 1; i++)
            {
                this.Subject.RegisterFailedLogin(userName);
                this.Subject.ShouldShowCaptcha(userName).ShouldBeFalse();
            }

            this.Subject.RegisterFailedLogin(userName);
            this.Subject.ShouldShowCaptcha(userName).ShouldBeTrue();
        }

        [Test]
        public void ShouldResetCaptchaRequirementOnSucessLogin()
        {
            var userName = Guid.NewGuid().ToString();
            
            for (int i = 0; i < NumberOfAttempts; i++)
            {
                this.Subject.RegisterFailedLogin(userName);
            }
            this.Subject.ShouldShowCaptcha(userName).ShouldBeTrue();
            this.Subject.ResetFailedLogin(userName);

            this.Subject.ShouldShowCaptcha(userName).ShouldBeFalse();
        }
    }
}
