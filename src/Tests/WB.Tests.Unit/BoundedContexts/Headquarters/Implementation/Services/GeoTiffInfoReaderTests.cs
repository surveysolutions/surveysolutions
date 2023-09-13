using System.IO;
using NUnit.Framework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.PixelFormats;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Implementation.Services;

[TestOf(typeof(GeoTiffInfoReader))]
public class GeoTiffInfoReaderTests
{
    [Test]
    public void when_check_image_tiff_file_on_geo_tiff_format()
    {
        using var image = new Image<Rgba32>(100, 100);
        using MemoryStream ms = new MemoryStream();
        image.Save(ms, TiffFormat.Instance);
        var bytes = ms.ToArray();

        var isGeoTiff = GeoTiffInfoReader.IsGeoTIFF(bytes);
        
        Assert.That(isGeoTiff, Is.False);
    }
    
    [Test]
    public void when_check_image_file_on_geo_tiff_format()
    {
        using var image = new Image<Rgba32>(100, 100);
        using MemoryStream ms = new MemoryStream();
        image.Save(ms, JpegFormat.Instance);
        var bytes = ms.ToArray();

        var isGeoTiff = GeoTiffInfoReader.IsGeoTIFF(bytes);
        
        Assert.That(isGeoTiff, Is.False);
    }
}