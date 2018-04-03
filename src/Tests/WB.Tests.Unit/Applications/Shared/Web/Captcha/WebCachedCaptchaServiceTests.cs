using System;
using FluentAssertions;
using NUnit.Framework;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Applications.Shared.Web.Captcha
{
    public class WebCachedCaptchaServiceTests
    {
        const int NumberOfAttempts = 5;
        
        [Test]
        public void When_FailingLogins_Should_NotRequireCaptchaForFirst5Attempts()
        {
            var subject = Create.Service.WebCacheBasedCaptchaService(NumberOfAttempts);
            var userName = Guid.NewGuid().ToString();

            subject.ShouldShowCaptcha(userName).Should().BeFalse();

            for (int i = 0; i < NumberOfAttempts - 1; i++)
            {
                subject.RegisterFailedLogin(userName);
                subject.ShouldShowCaptcha(userName).Should().BeFalse();
            }

            subject.RegisterFailedLogin(userName);
            subject.ShouldShowCaptcha(userName).Should().BeTrue();
        }

        [Test]
        public void When_ResetFailedLogin_Should_ResetCaptchaRequirement()
        {
            var subject = Create.Service.WebCacheBasedCaptchaService(NumberOfAttempts);
            var userName = Guid.NewGuid().ToString();
            
            for (int i = 0; i < NumberOfAttempts; i++)
            {
                subject.RegisterFailedLogin(userName);
            }
            subject.ShouldShowCaptcha(userName).Should().BeTrue();
            subject.ResetFailedLogin(userName);

            subject.ShouldShowCaptcha(userName).Should().BeFalse();
        }
    }
}
