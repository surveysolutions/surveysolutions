using System.IO;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
using NUnit.Framework;
using SkiaSharp;
using WB.Core.BoundedContexts.Headquarters.PdfInterview;
using WB.Core.Infrastructure.Exceptions;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.PdfInterview;

[TestOf(typeof(SkiaImageSource))]
public class SkiaImageSourceTests
{
    [SetUp]
    public void Setup()
    {
        ImageSource.ImageSourceImpl = new SkiaImageSource();
    }

    [Test]
    public void when_loading_not_an_image_should_throw_invalid_image_exception()
    {
        Assert.Throws<InvalidImageException>(() => ImageSource.FromBinary("image", () => new byte[] { 1, 2, 3, 4 }));
    }

    [Test]
    public void when_loading_png_should_be_transparent_and_have_original_size()
    {
        var source = ImageSource.FromBinary("image", () => CreateImage(SKEncodedImageFormat.Png, 3, 2));

        Assert.That(source.Transparent, Is.True);
        Assert.That(source.Width, Is.EqualTo(3));
        Assert.That(source.Height, Is.EqualTo(2));
    }

    [Test]
    public void when_loading_jpeg_should_not_be_transparent()
    {
        var source = ImageSource.FromBinary("image", () => CreateImage(SKEncodedImageFormat.Jpeg, 3, 2));

        Assert.That(source.Transparent, Is.False);
    }

    [Test]
    public void when_loading_from_stream_should_read_image()
    {
        var source = ImageSource.FromStream("image", () => new MemoryStream(CreateImage(SKEncodedImageFormat.Png, 4, 5)));

        Assert.That(source.Width, Is.EqualTo(4));
        Assert.That(source.Height, Is.EqualTo(5));
    }

    [Test]
    public void when_saving_as_jpeg_should_write_jpeg_content()
    {
        var source = ImageSource.FromBinary("image", () => CreateImage(SKEncodedImageFormat.Png, 3, 2));
        using var ms = new MemoryStream();

        source.SaveAsJpeg(ms);

        using var codec = SKCodec.Create(new SKMemoryStream(ms.ToArray()));
        Assert.That(codec.EncodedFormat, Is.EqualTo(SKEncodedImageFormat.Jpeg));
    }

    [Test]
    public void when_saving_as_pdf_bitmap_should_write_32_bit_bottom_up_bmp()
    {
        const int width = 3;
        const int height = 2;
        var source = ImageSource.FromBinary("image", () => CreateImage(SKEncodedImageFormat.Png, width, height));
        using var ms = new MemoryStream();

        source.SaveAsPdfBitmap(ms);

        var bmp = ms.ToArray();
        Assert.That(bmp[0], Is.EqualTo((byte)'B'));
        Assert.That(bmp[1], Is.EqualTo((byte)'M'));
        Assert.That(bmp.Length, Is.EqualTo(54 + width * height * 4));
        Assert.That(System.BitConverter.ToInt32(bmp, 2), Is.EqualTo(bmp.Length));
        Assert.That(System.BitConverter.ToInt32(bmp, 10), Is.EqualTo(54));
        Assert.That(System.BitConverter.ToInt32(bmp, 14), Is.EqualTo(40));
        Assert.That(System.BitConverter.ToInt32(bmp, 18), Is.EqualTo(width));
        Assert.That(System.BitConverter.ToInt32(bmp, 22), Is.EqualTo(height));
        Assert.That(System.BitConverter.ToInt16(bmp, 28), Is.EqualTo(32));
        Assert.That(System.BitConverter.ToInt32(bmp, 30), Is.EqualTo(0));

        // red pixel is stored as BGRA
        Assert.That(bmp[54..58], Is.EqualTo(new byte[] { 0, 0, 255, 255 }));
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
