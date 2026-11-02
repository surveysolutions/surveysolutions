using NUnit.Framework;
using SkiaSharp;
using WB.Core.Infrastructure.Exceptions;
using WB.Infrastructure.Native.Utils;

namespace WB.Tests.Unit.Infrastructure.Native;

[TestOf(typeof(SkiaImageDecoder))]
public class SkiaImageDecoderTests
{
    [Test]
    public void when_reading_info_of_image_with_huge_declared_size_should_return_size_without_decoding()
    {
        var content = PngTestData.WithDeclaredSize(20_000, 30_000);

        var info = SkiaImageDecoder.ReadInfo(content, out var format);

        Assert.That(format, Is.EqualTo(SKEncodedImageFormat.Png));
        Assert.That(info.Width, Is.EqualTo(20_000));
        Assert.That(info.Height, Is.EqualTo(30_000));
    }

    [Test]
    public void when_decoding_image_with_huge_declared_size_should_throw_before_allocating_memory()
    {
        // 600 megapixels, about 2.4 GB of memory for a file of a few dozen bytes
        var content = PngTestData.WithDeclaredSize(20_000, 30_000);

        var exception = Assert.Throws<InvalidImageException>(() => SkiaImageDecoder.Decode(content, out _));

        Assert.That(exception!.Message, Does.Contain("too large"));
    }

    [Test]
    public void when_image_has_pixels_in_budget_should_decode_it()
    {
        using var bitmap = SkiaImageDecoder.Decode(CreateImage(SKEncodedImageFormat.Png, 10, 10), out var format, maxPixels: 100);

        Assert.That(format, Is.EqualTo(SKEncodedImageFormat.Png));
        Assert.That(bitmap.Width, Is.EqualTo(10));
        Assert.That(bitmap.Height, Is.EqualTo(10));
    }

    [Test]
    public void when_image_has_more_pixels_than_budget_should_throw_invalid_image_exception()
    {
        var content = CreateImage(SKEncodedImageFormat.Png, 10, 10);

        var exception = Assert.Throws<InvalidImageException>(() => SkiaImageDecoder.Decode(content, out _, maxPixels: 99));

        Assert.That(exception!.Message, Does.Contain("too large"));
    }

    [Test]
    public void when_content_is_not_an_image_should_throw_invalid_image_exception()
    {
        Assert.Throws<InvalidImageException>(() => SkiaImageDecoder.ReadInfo(new byte[] { 1, 2, 3, 4 }, out _));
        Assert.Throws<InvalidImageException>(() => SkiaImageDecoder.Decode(new byte[] { 1, 2, 3, 4 }, out _));
    }

    [Test]
    public void when_image_is_truncated_should_throw_invalid_image_exception()
    {
        var content = CreateImage(SKEncodedImageFormat.Jpeg, 200, 200);

        var exception = Assert.Throws<InvalidImageException>(() => SkiaImageDecoder.Decode(content[..^2], out _));

        Assert.That(exception!.Message, Does.Contain("corrupted"));
    }

    private static byte[] CreateImage(SKEncodedImageFormat format, int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        bitmap.Erase(SKColors.Red);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(format, 90);
        return data.ToArray();
    }
}
