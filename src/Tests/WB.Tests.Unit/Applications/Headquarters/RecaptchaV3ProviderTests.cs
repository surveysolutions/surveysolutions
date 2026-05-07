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
        var minimumScore = 0.5m;
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
        var minimumScore = 0.5m;
        var recaptchaService = Mock.Of<IRecaptchaService>(s =>
            s.Validate(It.IsAny<HttpRequest>(), It.IsAny<bool>()) == Task.FromResult(new RecaptchaResponse { success = true, score = 0.3m }));
        var captchaConfig = Options.Create(new CaptchaConfig { RecaptchaV3MinimumScore = minimumScore });

        var provider = new RecaptchaV3Provider(recaptchaService, captchaConfig);
        var result = await provider.IsCaptchaValid(Mock.Of<HttpRequest>());

        result.Should().BeFalse();
    }

    [Test]
    public async Task when_validation_fails_should_return_false_regardless_of_score()
    {
        var recaptchaService = Mock.Of<IRecaptchaService>(s =>
            s.Validate(It.IsAny<HttpRequest>(), It.IsAny<bool>()) == Task.FromResult(new RecaptchaResponse { success = false, score = 0.9m }));
        var captchaConfig = Options.Create(new CaptchaConfig { RecaptchaV3MinimumScore = 0.5m });

        var provider = new RecaptchaV3Provider(recaptchaService, captchaConfig);
        var result = await provider.IsCaptchaValid(Mock.Of<HttpRequest>());

        result.Should().BeFalse();
    }
}
