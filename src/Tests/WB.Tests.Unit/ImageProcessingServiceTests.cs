using NUnit.Framework;
using SkiaSharp;
using WB.Core.Infrastructure.Exceptions;
using WB.UI.Shared.Web.Services;

namespace WB.Tests.Unit;

[TestOf(typeof(ImageProcessingService))]
public class ImageProcessingServiceTests
{
    private readonly ImageProcessingService service = new();

    [Test]
    public void when_validating_valid_image_should_not_throw()
    {
        var image = CreateImage(SKEncodedImageFormat.Png, 20, 10);

        Assert.DoesNotThrow(() => service.Validate(image));
    }

    [Test]
    public void when_validating_not_an_image_should_throw_invalid_image_exception()
    {
        Assert.Throws<InvalidImageException>(() => service.Validate(new byte[] { 1, 2, 3, 4 }));
    }

    [Test]
    public void when_resizing_not_an_image_should_throw_invalid_image_exception()
    {
        Assert.Throws<InvalidImageException>(() => service.ResizeImage(new byte[] { 1, 2, 3, 4 }, 100, 100));
    }

    [Test]
    public void when_resizing_should_keep_aspect_ratio_and_return_png()
    {
        var image = CreateImage(SKEncodedImageFormat.Jpeg, 200, 100);

        var resized = service.ResizeImage(image, height: 50, width: 1920);

        var (format, width, height) = ReadInfo(resized);
        Assert.That(format, Is.EqualTo(SKEncodedImageFormat.Png));
        Assert.That(width, Is.EqualTo(100));
        Assert.That(height, Is.EqualTo(50));
    }

    [Test]
    public void when_resizing_wide_image_should_fit_into_requested_width()
    {
        var image = CreateImage(SKEncodedImageFormat.Png, 400, 100);

        var resized = service.ResizeImage(image, height: 1000, width: 200);

        var (_, width, height) = ReadInfo(resized);
        Assert.That(width, Is.EqualTo(200));
        Assert.That(height, Is.EqualTo(50));
    }

    [TestCase(SKEncodedImageFormat.Jpeg)]
    [TestCase(SKEncodedImageFormat.Png)]
    public void when_resizing_keeping_format_should_return_same_format(SKEncodedImageFormat sourceFormat)
    {
        var image = CreateImage(sourceFormat, 100, 200);

        var resized = service.ResizeImageKeepingFormat(image, 50);

        var (format, width, height) = ReadInfo(resized);
        Assert.That(format, Is.EqualTo(sourceFormat));
        Assert.That(width, Is.EqualTo(25));
        Assert.That(height, Is.EqualTo(50));
    }

    [TestCase(SKEncodedImageFormat.Jpeg, "image/jpeg")]
    [TestCase(SKEncodedImageFormat.Png, "image/png")]
    [TestCase(SKEncodedImageFormat.Webp, "image/webp")]
    public void when_getting_mime_type_should_return_type_of_image_format(SKEncodedImageFormat format, string expectedMimeType)
    {
        var image = CreateImage(format, 10, 10);

        Assert.That(service.GetImageMimeType(image), Is.EqualTo(expectedMimeType));
    }

    [Test]
    public void when_getting_mime_type_of_not_an_image_should_throw_invalid_image_exception()
    {
        Assert.Throws<InvalidImageException>(() => service.GetImageMimeType(new byte[] { 1, 2, 3, 4 }));
    }

    private static byte[] CreateImage(SKEncodedImageFormat format, int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        bitmap.Erase(SKColors.Red);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(format, 90);
        return data.ToArray();
    }

    private static (SKEncodedImageFormat format, int width, int height) ReadInfo(byte[] content)
    {
        using var codec = SKCodec.Create(new SKMemoryStream(content));
        return (codec.EncodedFormat, codec.Info.Width, codec.Info.Height);
    }
}
