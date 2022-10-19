using System;
using System.Linq;
using NUnit.Framework;
using SixLabors.Fonts;
using WB.UI.Shared.Web.Captcha;

namespace WB.Tests.Unit;

[TestOf(typeof(CaptchaImageGenerator))]
public class CaptchaImageGeneratorTests
{
    [Test]
    public void when_generate_then_should_return_result_without_exception()
    {
        string code = "12345";
        var fontFamily = SystemFonts.Families.First();

        var captchaImageGenerator = new CaptchaImageGenerator();
        captchaImageGenerator.ChangeFonts(fontFamily.Name);

        var imageContent = captchaImageGenerator.Generate(code);
        
        Assert.That(imageContent, Is.Not.Null);
        Assert.That(imageContent.Length, Is.Not.EqualTo(0));
    }
}