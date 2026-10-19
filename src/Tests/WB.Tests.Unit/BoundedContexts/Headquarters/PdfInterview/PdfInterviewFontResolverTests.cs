using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SkiaSharp;
using WB.Core.BoundedContexts.Headquarters.PdfInterview;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.PdfInterview;

[TestOf(typeof(PdfInterviewFontResolver))]
public class PdfInterviewFontResolverTests
{
    private string tempDirectory = string.Empty;

    [SetUp]
    public void SetUp()
    {
        tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(tempDirectory))
            Directory.Delete(tempDirectory, true);
    }

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

    [TestCase(false, new[] { 300, 500, 400 }, 400)]
    [TestCase(false, new[] { 300, 500 }, 500)]
    [TestCase(false, new[] { 200, 300 }, 300)]
    [TestCase(false, new[] { 600, 700, 900 }, 600)]
    [TestCase(true, new[] { 400, 700, 900 }, 700)]
    [TestCase(true, new[] { 400, 600, 700 }, 700)]
    [TestCase(true, new[] { 400, 500 }, 500)]
    [TestCase(true, new[] { 400, 800, 900 }, 800)]
    public void when_family_has_faces_of_different_weights_should_resolve_the_closest_to_requested(bool bold, int[] weights, int expectedWeight)
    {
        // file names are in the order of weights, so lighter faces are found before the regular one
        foreach (var weight in weights)
            CreateFontFile(tempDirectory, $"face-{weight}.ttf", weight: weight);
        var resolver = new PdfInterviewFontResolver(new[] { tempDirectory });

        var info = resolver.ResolveTypeface("Noto Sans", bold, false);

        Assert.That(Path.GetFileName(info!.FaceName), Is.EqualTo($"face-{expectedWeight}.ttf"));
        Assert.That(info.MustSimulateBold, Is.EqualTo(bold && expectedWeight < 600));
    }

    [Test]
    public void when_family_has_condensed_and_normal_faces_should_ignore_condensed_one()
    {
        CreateFontFile(tempDirectory, "a-condensed.ttf", width: (int)SKFontStyleWidth.Condensed);
        CreateFontFile(tempDirectory, "b-normal.ttf");
        var resolver = new PdfInterviewFontResolver(new[] { tempDirectory });

        var info = resolver.ResolveTypeface("Noto Sans", false, false);

        Assert.That(Path.GetFileName(info!.FaceName), Is.EqualTo("b-normal.ttf"));
    }

    [Test]
    public void when_family_has_only_condensed_face_should_still_resolve_it()
    {
        CreateFontFile(tempDirectory, "condensed.ttf", width: (int)SKFontStyleWidth.Condensed);
        var resolver = new PdfInterviewFontResolver(new[] { tempDirectory });

        var info = resolver.ResolveTypeface("Noto Sans", false, false);

        Assert.That(Path.GetFileName(info!.FaceName), Is.EqualTo("condensed.ttf"));
    }

    [Test]
    public void when_font_extension_is_in_upper_case_should_find_the_font()
    {
        CreateFontFile(tempDirectory, "NOTOSANS.TTF");
        var resolver = new PdfInterviewFontResolver(new[] { tempDirectory });

        var info = resolver.ResolveTypeface("Noto Sans", false, false);

        Assert.That(Path.GetFileName(info!.FaceName), Is.EqualTo("NOTOSANS.TTF"));
    }

    [Test]
    public void when_same_face_is_in_several_directories_should_use_the_first_directory()
    {
        var first = Path.Combine(tempDirectory, "z-first");
        var second = Path.Combine(tempDirectory, "a-second");
        CreateFontFile(first, "NotoSans.ttf");
        CreateFontFile(second, "NotoSans.ttf");
        var resolver = new PdfInterviewFontResolver(new[] { first, second });

        var info = resolver.ResolveTypeface("Noto Sans", false, false);

        Assert.That(Path.GetDirectoryName(info!.FaceName), Is.EqualTo(first));
    }

    [Test]
    public void when_getting_font_that_was_not_found_by_the_resolver_should_not_read_the_file()
    {
        var resolver = new PdfInterviewFontResolver(new[] { GetProductFontsDirectory() });
        resolver.ResolveTypeface("Noto Sans", false, false);

        var anotherFile = Path.Combine(tempDirectory, "secret.txt");
        File.WriteAllText(anotherFile, "secret");

        Assert.That(resolver.GetFont(anotherFile), Is.Null);
        Assert.That(resolver.GetFont(Path.Combine(GetProductFontsDirectory(), "..", "..", "..", "..", "LICENSE.md")), Is.Null);
    }

    [TestCase("fonts.ttc")]
    [TestCase("FONTS.TTC")]
    public void when_font_is_in_collection_should_return_its_face_as_standalone_font(string collectionFileName)
    {
        File.WriteAllBytes(Path.Combine(tempDirectory, collectionFileName), BuildCollection(
            File.ReadAllBytes(Path.Combine(GetProductFontsDirectory(), "NotoSans-Bold.ttf"))));
        var resolver = new PdfInterviewFontResolver(new[] { tempDirectory });

        var info = resolver.ResolveTypeface("Noto Sans", true, false)!;

        Assert.That(info.MustSimulateBold, Is.False);
        AssertStandaloneFont(resolver.GetFont(info.FaceName), 700);
    }

    [Test]
    public void when_collection_has_several_faces_should_resolve_each_of_them()
    {
        var collection = Path.Combine(tempDirectory, "fonts.ttc");
        File.WriteAllBytes(collection, BuildCollection(
            File.ReadAllBytes(Path.Combine(GetProductFontsDirectory(), "NotoSans-Regular.ttf")),
            File.ReadAllBytes(Path.Combine(GetProductFontsDirectory(), "NotoSans-Bold.ttf"))));
        using (var second = SKTypeface.FromFile(collection, 1))
        {
            // Skia on macOS opens only the first face of a collection
            if (second == null)
                Assert.Ignore("Skia can not open faces of true type collections except the first one on this platform");
        }

        var resolver = new PdfInterviewFontResolver(new[] { tempDirectory });
        var regular = resolver.ResolveTypeface("Noto Sans", false, false)!;
        var bold = resolver.ResolveTypeface("Noto Sans", true, false)!;

        Assert.That(regular.FaceName, Is.Not.EqualTo(bold.FaceName));
        AssertStandaloneFont(resolver.GetFont(regular.FaceName), 400);
        AssertStandaloneFont(resolver.GetFont(bold.FaceName), 700);
    }

    private static void AssertStandaloneFont(byte[] font, int expectedWeight)
    {
        Assert.That(font, Is.Not.Null);
        Assert.That(font.AsSpan(0, 4).ToArray(), Is.EqualTo(new byte[] { 0, 1, 0, 0 }));

        using var data = SKData.CreateCopy(font);
        using var typeface = SKTypeface.FromData(data);
        Assert.That(typeface, Is.Not.Null);
        Assert.That(typeface.FamilyName, Is.EqualTo("Noto Sans"));
        Assert.That(typeface.FontWeight, Is.EqualTo(expectedWeight));
    }

    /// <summary>
    /// Creates a copy of Noto Sans Regular with changed weight and width, they are read by Skia from OS/2 table.
    /// </summary>
    private static string CreateFontFile(string directory, string fileName, int? weight = null, int? width = null)
    {
        Directory.CreateDirectory(directory);
        var font = File.ReadAllBytes(Path.Combine(GetProductFontsDirectory(), "NotoSans-Regular.ttf"));

        var tableCount = BinaryPrimitives.ReadUInt16BigEndian(font.AsSpan(4));
        for (var i = 0; i < tableCount; i++)
        {
            var record = font.AsSpan(12 + 16 * i);
            if (record[..4].SequenceEqual("OS/2"u8))
            {
                var table = (int)BinaryPrimitives.ReadUInt32BigEndian(record[8..]);
                if (weight.HasValue)
                    BinaryPrimitives.WriteUInt16BigEndian(font.AsSpan(table + 4), (ushort)weight.Value);
                if (width.HasValue)
                    BinaryPrimitives.WriteUInt16BigEndian(font.AsSpan(table + 6), (ushort)width.Value);
            }
        }

        var path = Path.Combine(directory, fileName);
        File.WriteAllBytes(path, font);

        using var typeface = SKTypeface.FromFile(path);
        if ((weight.HasValue && typeface?.FontWeight != weight) || (width.HasValue && typeface?.FontWidth != width))
            Assert.Ignore("Skia does not read weight and width of a font from OS/2 table on this platform");

        return path;
    }

    /// <summary>
    /// Builds a true type collection: font directories are followed by their tables, offsets are from the start of the file.
    /// </summary>
    private static byte[] BuildCollection(params byte[][] fonts)
    {
        var header = new byte[12 + 4 * fonts.Length];
        "ttcf"u8.CopyTo(header);
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(4), 0x00010000);
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(8), (uint)fonts.Length);

        using var output = new MemoryStream();
        output.Write(header);

        for (var fontIndex = 0; fontIndex < fonts.Length; fontIndex++)
        {
            var font = fonts[fontIndex];
            BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(12 + 4 * fontIndex), (uint)output.Length);

            var tableCount = BinaryPrimitives.ReadUInt16BigEndian(font.AsSpan(4));
            var directorySize = 12 + 16 * tableCount;
            var directory = font.AsSpan(0, directorySize).ToArray();

            using var tables = new MemoryStream();
            for (var i = 0; i < tableCount; i++)
            {
                var record = directory.AsSpan(12 + 16 * i);
                var offset = (int)BinaryPrimitives.ReadUInt32BigEndian(record[8..]);
                var length = (int)BinaryPrimitives.ReadUInt32BigEndian(record[12..]);

                BinaryPrimitives.WriteUInt32BigEndian(record[8..], (uint)(output.Length + directorySize + tables.Length));
                tables.Write(font, offset, length);
                tables.Write(new byte[(4 - length % 4) % 4]);
            }

            output.Write(directory);
            tables.WriteTo(output);
        }

        var collection = output.ToArray();
        header.CopyTo(collection, 0);
        return collection;
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
