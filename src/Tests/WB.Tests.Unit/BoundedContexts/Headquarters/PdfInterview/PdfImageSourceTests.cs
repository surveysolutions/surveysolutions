using System;
using NUnit.Framework;
using SkiaSharp;
using WB.Core.BoundedContexts.Headquarters.PdfInterview;
using WB.Core.Infrastructure.Exceptions;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.PdfInterview;

[TestOf(typeof(PdfImageSource))]
public class PdfImageSourceTests
{
    [Test]
    public void when_content_is_not_an_image_should_throw_invalid_image_exception()
    {
        Assert.Throws<InvalidImageException>(() => PdfImageSource.FromBinary(new byte[] { 1, 2, 3, 4 }));
    }

    [Test]
    public void when_image_declares_huge_size_should_throw_invalid_image_exception_instead_of_allocating_memory()
    {
        // 600 megapixels, about 2.4 GB of memory for a file of a few dozen bytes
        var content = PngTestData.WithDeclaredSize(20_000, 30_000);

        Assert.That(() => PdfImageSource.FromBinary(content), Throws.TypeOf<InvalidImageException>().With.Message.Contains("too large"));
    }

    [Test]
    public void when_content_is_incomplete_image_should_throw_invalid_image_exception()
    {
        var image = CreateImage(SKEncodedImageFormat.Jpeg, 200, 200);

        Assert.Throws<InvalidImageException>(() => PdfImageSource.FromBinary(image[..^2]));
    }

    [Test]
    public void when_image_is_png_should_keep_png_to_preserve_transparency()
    {
        var source = PdfImageSource.FromBinary(CreateImage(SKEncodedImageFormat.Png, 3, 2));

        var (format, width, height) = ReadInfo(source);
        Assert.That(format, Is.EqualTo(SKEncodedImageFormat.Png));
        Assert.That(width, Is.EqualTo(3));
        Assert.That(height, Is.EqualTo(2));
    }

    [TestCase(SKEncodedImageFormat.Jpeg)]
    [TestCase(SKEncodedImageFormat.Webp)]
    public void when_image_is_not_png_should_convert_it_to_jpeg(SKEncodedImageFormat sourceFormat)
    {
        var source = PdfImageSource.FromBinary(CreateImage(sourceFormat, 4, 5));

        var (format, width, height) = ReadInfo(source);
        Assert.That(format, Is.EqualTo(SKEncodedImageFormat.Jpeg));
        Assert.That(width, Is.EqualTo(4));
        Assert.That(height, Is.EqualTo(5));
    }

    private static byte[] CreateImage(SKEncodedImageFormat format, int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        bitmap.Erase(SKColors.Red);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(format, 90);
        return data.ToArray();
    }

    private static (SKEncodedImageFormat format, int width, int height) ReadInfo(string migraDocSource)
    {
        Assert.That(migraDocSource, Does.StartWith("base64:"));
        var content = Convert.FromBase64String(migraDocSource["base64:".Length..]);
        using var codec = SKCodec.Create(new SKMemoryStream(content));
        return (codec.EncodedFormat, codec.Info.Width, codec.Info.Height);
    }
}
