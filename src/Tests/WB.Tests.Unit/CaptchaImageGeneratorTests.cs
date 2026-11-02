using System.Linq;
using NUnit.Framework;
using SkiaSharp;
using WB.UI.Shared.Web.Captcha;

namespace WB.Tests.Unit;

[TestOf(typeof(CaptchaImageGenerator))]
public class CaptchaImageGeneratorTests
{
    [Test]
    //[Ignore("To be fixed on main branch")]
    public void when_generate_then_should_return_result_without_exception()
    {
        string code = "12345";

        var fontFamily = SKFontManager.Default.GetFontFamilies().FirstOrDefault();
        if (fontFamily == null)
            Assert.Ignore("There are no installed fonts");

        var captchaImageGenerator = new CaptchaImageGenerator();
        captchaImageGenerator.SetFonts(fontFamily!);

        var imageContent = captchaImageGenerator.Generate(code);
        
        Assert.That(imageContent, Is.Not.Null);
        Assert.That(imageContent.Length, Is.Not.EqualTo(0));
    }
}
