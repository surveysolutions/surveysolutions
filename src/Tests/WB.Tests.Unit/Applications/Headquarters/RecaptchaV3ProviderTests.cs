using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using reCAPTCHA.AspNetCore;
using WB.UI.Headquarters.Services;
using WB.UI.Shared.Web.Services;

namespace WB.Tests.Unit.Applications.Headquarters;

[TestOf(typeof(RecaptchaV3Provider))]
public class RecaptchaV3ProviderTests
{
    [Test]
    public async Task when_validation_succeeds_and_score_equals_threshold_should_return_true()
    {
        var minimumScore = 0.5;
        var recaptchaService = Mock.Of<IRecaptchaService>(s =>
            s.Validate(It.IsAny<HttpRequest>(), It.IsAny<bool>()) == Task.FromResult(new RecaptchaResponse { success = true, score = minimumScore }));
        var captchaConfig = Options.Create(new CaptchaConfig { RecaptchaV3MinimumScore = minimumScore });

        var provider = new RecaptchaV3Provider(recaptchaService, captchaConfig);
        var result = await provider.IsCaptchaValid(Mock.Of<HttpRequest>());

        result.Should().BeTrue();
    }

    [Test]
    public async Task when_validation_succeeds_and_score_is_below_threshold_should_return_false()
    {
        var minimumScore = 0.5;
        var recaptchaService = Mock.Of<IRecaptchaService>(s =>
            s.Validate(It.IsAny<HttpRequest>(), It.IsAny<bool>()) == Task.FromResult(new RecaptchaResponse { success = true, score = 0.3 }));
        var captchaConfig = Options.Create(new CaptchaConfig { RecaptchaV3MinimumScore = minimumScore });

        var provider = new RecaptchaV3Provider(recaptchaService, captchaConfig);
        var result = await provider.IsCaptchaValid(Mock.Of<HttpRequest>());

        result.Should().BeFalse();
    }

    [Test]
    public async Task when_validation_fails_should_return_false_regardless_of_score()
    {
        var recaptchaService = Mock.Of<IRecaptchaService>(s =>
            s.Validate(It.IsAny<HttpRequest>(), It.IsAny<bool>()) == Task.FromResult(new RecaptchaResponse { success = false, score = 0.9 }));
        var captchaConfig = Options.Create(new CaptchaConfig { RecaptchaV3MinimumScore = 0.5 });

        var provider = new RecaptchaV3Provider(recaptchaService, captchaConfig);
        var result = await provider.IsCaptchaValid(Mock.Of<HttpRequest>());

        result.Should().BeFalse();
    }

    [Test]
    public async Task when_web_interview_captcha_action_is_start_should_return_true()
    {
        var recaptchaService = Mock.Of<IRecaptchaService>(s =>
            s.Validate(It.IsAny<HttpRequest>(), It.IsAny<bool>()) == Task.FromResult(new RecaptchaResponse
            {
                success = true,
                score = 0.9,
                action = "start"
            }));
        var captchaConfig = Options.Create(new CaptchaConfig { RecaptchaV3MinimumScore = 0.5 });
        var request = new DefaultHttpContext().Request;
        request.Path = "/WebInterview/Resume/11111111-1111-1111-1111-111111111111";

        var provider = new RecaptchaV3Provider(recaptchaService, captchaConfig);
        var result = await provider.IsCaptchaValid(request);

        result.Should().BeTrue();
    }

    [Test]
    public async Task when_web_interview_captcha_action_does_not_match_should_return_false()
    {
        var recaptchaService = Mock.Of<IRecaptchaService>(s =>
            s.Validate(It.IsAny<HttpRequest>(), It.IsAny<bool>()) == Task.FromResult(new RecaptchaResponse
            {
                success = true,
                score = 0.9,
                action = "other"
            }));
        var captchaConfig = Options.Create(new CaptchaConfig { RecaptchaV3MinimumScore = 0.5 });
        var request = new DefaultHttpContext().Request;
        request.Path = "/WebInterview/Resume/11111111-1111-1111-1111-111111111111";

        var provider = new RecaptchaV3Provider(recaptchaService, captchaConfig);
        var result = await provider.IsCaptchaValid(request);

        result.Should().BeFalse();
    }

    [Test]
    public async Task when_logon_captcha_action_is_login_should_return_true()
    {
        var recaptchaService = Mock.Of<IRecaptchaService>(s =>
            s.Validate(It.IsAny<HttpRequest>(), It.IsAny<bool>()) == Task.FromResult(new RecaptchaResponse
            {
                success = true,
                score = 0.9,
                action = "login"
            }));
        var captchaConfig = Options.Create(new CaptchaConfig { RecaptchaV3MinimumScore = 0.5 });
        var request = new DefaultHttpContext().Request;
        request.Path = "/Account/LogOn";

        var provider = new RecaptchaV3Provider(recaptchaService, captchaConfig);
        var result = await provider.IsCaptchaValid(request);

        result.Should().BeTrue();
    }

    [Test]
    public async Task when_logon_captcha_action_does_not_match_should_return_false()
    {
        var recaptchaService = Mock.Of<IRecaptchaService>(s =>
            s.Validate(It.IsAny<HttpRequest>(), It.IsAny<bool>()) == Task.FromResult(new RecaptchaResponse
            {
                success = true,
                score = 0.9,
                action = "start"
            }));
        var captchaConfig = Options.Create(new CaptchaConfig { RecaptchaV3MinimumScore = 0.5 });
        var request = new DefaultHttpContext().Request;
        request.Path = "/Account/LogOn";

        var provider = new RecaptchaV3Provider(recaptchaService, captchaConfig);
        var result = await provider.IsCaptchaValid(request);

        result.Should().BeFalse();
    }
}
