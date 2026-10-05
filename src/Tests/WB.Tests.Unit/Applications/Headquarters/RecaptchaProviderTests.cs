using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using reCAPTCHA.AspNetCore;
using WB.UI.Headquarters.Services;

namespace WB.Tests.Unit.Applications.Headquarters;

[TestOf(typeof(RecaptchaProvider))]
public class RecaptchaProviderTests
{
    [Test]
    public async Task when_validation_succeeds_should_return_true()
    {
        var recaptchaService = Mock.Of<IRecaptchaService>(s =>
            s.Validate(It.IsAny<HttpRequest>(), It.IsAny<bool>()) == Task.FromResult(new RecaptchaResponse { success = true }));

        var provider = new RecaptchaProvider(recaptchaService);
        var result = await provider.IsCaptchaValid(Mock.Of<HttpRequest>());

        result.Should().BeTrue();
    }

    [Test]
    public async Task when_validation_fails_should_return_false()
    {
        var recaptchaService = Mock.Of<IRecaptchaService>(s =>
            s.Validate(It.IsAny<HttpRequest>(), It.IsAny<bool>()) == Task.FromResult(new RecaptchaResponse { success = false }));

        var provider = new RecaptchaProvider(recaptchaService);
        var result = await provider.IsCaptchaValid(Mock.Of<HttpRequest>());

        result.Should().BeFalse();
    }
}
