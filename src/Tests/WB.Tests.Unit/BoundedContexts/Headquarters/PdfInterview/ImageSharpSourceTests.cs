using System.IO;
using NUnit.Framework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using WB.Core.BoundedContexts.Headquarters.PdfInterview;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.PdfInterview;

[TestOf(typeof(ImageSharpSource<>))]
public class ImageSharpSourceTests
{
    [Test]
    public void when_image_exceeds_max_dimension_should_downscale_to_fit_within_limit()
    {
        var imageBytes = CreateJpegImageBytes(width: 2000, height: 1500);

        var source = ImageSharpSource<Rgba32>.FromBinaryResized("photo.jpg", imageBytes, maxDimension: 1024);

        Assert.That(source.Width, Is.LessThanOrEqualTo(1024));
        Assert.That(source.Height, Is.LessThanOrEqualTo(1024));
    }

    [Test]
    public void when_image_exceeds_max_dimension_should_preserve_aspect_ratio()
    {
        // 2000x1000 -> longest edge 2000 -> scaled to 1024x512
        var imageBytes = CreateJpegImageBytes(width: 2000, height: 1000);

        var source = ImageSharpSource<Rgba32>.FromBinaryResized("photo.jpg", imageBytes, maxDimension: 1024);

        Assert.That(source.Width, Is.EqualTo(1024));
        Assert.That(source.Height, Is.EqualTo(512));
    }

    [Test]
    public void when_image_is_within_max_dimension_should_not_upscale()
    {
        var imageBytes = CreateJpegImageBytes(width: 500, height: 400);

        var source = ImageSharpSource<Rgba32>.FromBinaryResized("small.jpg", imageBytes, maxDimension: 1024);

        Assert.That(source.Width, Is.EqualTo(500));
        Assert.That(source.Height, Is.EqualTo(400));
    }

    [Test]
    public void when_image_is_square_and_exceeds_max_dimension_should_downscale_to_exact_max()
    {
        var imageBytes = CreateJpegImageBytes(width: 2048, height: 2048);

        var source = ImageSharpSource<Rgba32>.FromBinaryResized("square.jpg", imageBytes, maxDimension: 1024);

        Assert.That(source.Width, Is.EqualTo(1024));
        Assert.That(source.Height, Is.EqualTo(1024));
    }

    [Test]
    public void when_image_is_resized_source_name_should_match_provided_name()
    {
        var imageBytes = CreateJpegImageBytes(width: 2000, height: 2000);

        var source = ImageSharpSource<Rgba32>.FromBinaryResized("my_photo.jpg", imageBytes, maxDimension: 1024);

        Assert.That(source.Name, Is.EqualTo("my_photo.jpg"));
    }

    private static byte[] CreateJpegImageBytes(int width, int height)
    {
        using var image = new Image<Rgb24>(width, height);
        using var ms = new MemoryStream();
        image.Save(ms, JpegFormat.Instance);
        return ms.ToArray();
    }
}
