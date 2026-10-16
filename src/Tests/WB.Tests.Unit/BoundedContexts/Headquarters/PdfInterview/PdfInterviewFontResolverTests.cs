using System;
using System.IO;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.PdfInterview;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.PdfInterview;

[TestOf(typeof(PdfInterviewFontResolver))]
public class PdfInterviewFontResolverTests
{
    [TestCase(false, false, "NotoSans-Regular.ttf")]
    [TestCase(true, false, "NotoSans-Bold.ttf")]
    [TestCase(false, true, "NotoSans-Italic.ttf")]
    public void when_font_family_is_installed_should_resolve_face_with_requested_style(bool bold, bool italic, string expectedFile)
    {
        var resolver = new PdfInterviewFontResolver(new[] { GetProductFontsDirectory() });

        var info = resolver.ResolveTypeface("Noto Sans", bold, italic);

        Assert.That(info, Is.Not.Null);
        Assert.That(Path.GetFileName(info!.FaceName), Is.EqualTo(expectedFile));
        Assert.That(info.MustSimulateBold, Is.False);
        Assert.That(info.MustSimulateItalic, Is.False);
    }

    [Test]
    public void when_style_is_not_installed_should_use_closest_face_and_simulate_missing_style()
    {
        var resolver = new PdfInterviewFontResolver(new[] { GetProductFontsDirectory() });

        var info = resolver.ResolveTypeface("Noto Sans", isBold: true, isItalic: true);

        Assert.That(info, Is.Not.Null);
        Assert.That(Path.GetFileName(info!.FaceName), Is.EqualTo("NotoSans-Bold.ttf"));
        Assert.That(info.MustSimulateItalic, Is.True);
    }

    [Test]
    public void when_font_list_is_requested_should_use_first_installed_family()
    {
        var resolver = new PdfInterviewFontResolver(new[] { GetProductFontsDirectory() });

        var info = resolver.ResolveTypeface("Not Existing Font, noto sans", false, false);

        Assert.That(info, Is.Not.Null);
        Assert.That(Path.GetFileName(info!.FaceName), Is.EqualTo("NotoSans-Regular.ttf"));
    }

    [Test]
    public void when_font_family_is_unknown_should_fallback_to_default_family()
    {
        var resolver = new PdfInterviewFontResolver(new[] { GetProductFontsDirectory() });

        var info = resolver.ResolveTypeface("Not Existing Font", false, false);

        Assert.That(info, Is.Not.Null);
        Assert.That(Path.GetFileName(info!.FaceName), Is.EqualTo("NotoSans-Regular.ttf"));
    }

    [Test]
    public void when_no_fonts_are_installed_should_return_null()
    {
        var resolver = new PdfInterviewFontResolver(new[] { Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()) });

        Assert.That(resolver.ResolveTypeface("Arial", false, false), Is.Null);
    }

    [Test]
    public void when_getting_font_should_return_truetype_file_content()
    {
        var resolver = new PdfInterviewFontResolver(new[] { GetProductFontsDirectory() });
        var info = resolver.ResolveTypeface("Noto Sans", false, false);

        var font = resolver.GetFont(info!.FaceName);

        Assert.That(font, Is.Not.Null);
        Assert.That(font!.AsSpan(0, 4).ToArray(), Is.EqualTo(new byte[] { 0, 1, 0, 0 }));
    }

    private static string GetProductFontsDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            var fonts = Path.Combine(directory.FullName, "installer", "src", "SurveySolutionsProduct", "Fonts");
            if (Directory.Exists(fonts))
                return fonts;

            directory = directory.Parent;
        }

        Assert.Ignore("Product fonts directory is not found");
        return string.Empty;
    }
}
