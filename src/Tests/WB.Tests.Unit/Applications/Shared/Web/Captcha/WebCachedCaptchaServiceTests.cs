using System;
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
            this.Subject = Create.Service.WebCacheBasedCaptchaService(NumberOfAttempts);
        }

        [Test]
        public void When_FailingLogins_Should_NotRequireCaptchaForFirst5Attempts()
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
        public void When_ResetFailedLogin_Should_ResetCaptchaRequirement()
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
