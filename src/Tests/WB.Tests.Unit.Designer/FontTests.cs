using System.Runtime.CompilerServices;
using NUnit.Framework;
using SixLabors.Fonts;

namespace WB.Tests.Unit.Designer;

public class FontTests
{
    [Test]
    public void check_Noto_Sans_font()
    {
        var tryGet = SystemFonts.Collection.TryGet("Noto Sans", out var fontFamily);

        Assert.That(tryGet, Is.True);
        Assert.That(fontFamily, Is.Not.Null);
        Assert.That(fontFamily.Name, Is.EqualTo("Noto Sans"));
    }
}
