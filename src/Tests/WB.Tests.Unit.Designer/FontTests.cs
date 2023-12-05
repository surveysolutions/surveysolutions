using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ClosedXML.Graphics;
using NUnit.Framework;
using SixLabors.Fonts;
using SixLabors.Fonts.Unicode;

namespace WB.Tests.Unit.Designer;

public class FontTests
{
    [Test]
    public void check_Noto_Sans_font()
    {
        var tryGet = SystemFonts.Collection.TryGet("Noto Sans Regular", out var fontFamily);

        Assert.That(tryGet, Is.True);
        Assert.That(fontFamily, Is.Not.Null);
        Assert.That(fontFamily.Name, Is.EqualTo("Noto Sans Regular"));
    }
    
    [Test]
    public void check_first_fonts()
    {
        var fontFamily = SystemFonts.Collection.Families.First();
        
        Assert.That(fontFamily, Is.Not.Null);
    }
    
    [Test]
    public void check_all_fonts_fonts()
    {
        Console.WriteLine("All fonts: ");
        foreach (var collectionFamily in SystemFonts.Collection.Families)
        {
            var font = new Font(collectionFamily, 10);
            Console.Write(font.Name + ":  ");

            try
            {
                TestFont(font);
                Console.WriteLine("OK");
            }
            catch (Exception e)
            {
                Console.WriteLine("Fail");
            }
        }
        
        foreach (var collectionFamily in SystemFonts.Collection.Families)
        {
            var font = new Font(collectionFamily, 10);

            try
            {
                TestFont(font);
            }
            catch (Exception e)
            {
                Console.WriteLine("Problem with font: " + font.Name);
                Console.WriteLine(e);
                //throw;
            }
        }
        
        var fontFamily = SystemFonts.Collection.Families.First();
        
        Assert.That(fontFamily, Is.Not.Null);
    }

    private static void TestFont(Font font)
    {
        int val1 = int.MinValue;
        for (char ch = '0'; ch <= '9'; ++ch)
        {
            IReadOnlyList<GlyphMetrics> metrics;
            if (font.FontMetrics.TryGetGlyphMetrics(new CodePoint(ch), TextAttributes.None, TextDecorations.None,
                    LayoutMode.HorizontalTopBottom, ColorFontSupport.None, out metrics))
            {
                int val2 = 0;
                foreach (GlyphMetrics glyphMetrics in (IEnumerable<GlyphMetrics>)metrics)
                    val2 += (int)glyphMetrics.AdvanceWidth;
                val1 = Math.Max(val1, val2);
            }
        }  
    }
}
