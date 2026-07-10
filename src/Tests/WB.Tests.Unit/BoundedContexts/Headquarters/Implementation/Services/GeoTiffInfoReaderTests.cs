using System;
using System.Collections.Generic;
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

    // ── IsGeoTIFF file-path overload ─────────────────────────────────────────

    [Test]
    public void IsGeoTIFF_plain_tiff_returns_false()
    {
        string path = WriteTempTiff(TiffBuilder.PlainTiff(4, 4));
        try { Assert.That(GeoTiffInfoReader.IsGeoTIFF(path), Is.False); }
        finally { File.Delete(path); }
    }

    [Test]
    public void IsGeoTIFF_geotiff_with_geo_key_directory_returns_true()
    {
        string path = WriteTempTiff(TiffBuilder.GeoTiffWithCrsOnly(4, 4, WGS84GeoKeys()));
        try { Assert.That(GeoTiffInfoReader.IsGeoTIFF(path), Is.True); }
        finally { File.Delete(path); }
    }

    // ── TryReadGeoTiffBounds ─────────────────────────────────────────────────

    [Test]
    public void TryReadGeoTiffBounds_plain_tiff_without_geotiff_tags_returns_false()
    {
        string path = WriteTempTiff(TiffBuilder.PlainTiff(4, 4));
        try
        {
            bool result = GeoTiffInfoReader.TryReadGeoTiffBounds(path, out _, out _, out _, out _);
            Assert.That(result, Is.False);
        }
        finally { File.Delete(path); }
    }

    [Test]
    public void TryReadGeoTiffBounds_geotiff_with_crs_but_no_model_tags_returns_false()
    {
        // Has GeoKeyDirectory (valid GeoTIFF) but no ModelPixelScale or ModelTiepoint
        string path = WriteTempTiff(TiffBuilder.GeoTiffWithCrsOnly(4, 4, WGS84GeoKeys()));
        try
        {
            bool result = GeoTiffInfoReader.TryReadGeoTiffBounds(path, out _, out _, out _, out _);
            Assert.That(result, Is.False);
        }
        finally { File.Delete(path); }
    }

    [Test]
    public void TryReadGeoTiffBounds_wgs84_with_pixel_scale_and_tiepoint_returns_correct_bounds()
    {
        // Top-left at (-10°, 50°), pixel size 0.1°, 10×10 pixels
        // → expected bounds: x ∈ [-10, -9], y ∈ [49, 50]
        string path = WriteTempTiff(TiffBuilder.GeoTiffWithPixelScaleAndTiepoint(
            width: 10, height: 10,
            scaleX: 0.1, scaleY: 0.1,
            originX: -10.0, originY: 50.0,
            geoKeyDirectory: WGS84GeoKeys()));
        try
        {
            bool result = GeoTiffInfoReader.TryReadGeoTiffBounds(
                path, out double xMin, out double yMin, out double xMax, out double yMax);

            Assert.That(result, Is.True);
            Assert.That(xMin, Is.EqualTo(-10.0).Within(1e-9));
            Assert.That(xMax, Is.EqualTo(-9.0).Within(1e-9));
            Assert.That(yMin, Is.EqualTo(49.0).Within(1e-9));
            Assert.That(yMax, Is.EqualTo(50.0).Within(1e-9));
        }
        finally { File.Delete(path); }
    }

    [Test]
    public void TryReadGeoTiffBounds_wgs84_with_transformation_matrix_returns_correct_bounds()
    {
        // Same geometry as above expressed via ModelTransformationTag (row-major 4×4 affine matrix)
        var matrix = new double[]
        {
             0.1,  0.0, 0.0, -10.0,
             0.0, -0.1, 0.0,  50.0,
             0.0,  0.0, 0.0,   0.0,
             0.0,  0.0, 0.0,   1.0,
        };
        string path = WriteTempTiff(TiffBuilder.GeoTiffWithTransformationMatrix(
            width: 10, height: 10,
            transformMatrix: matrix,
            geoKeyDirectory: WGS84GeoKeys()));
        try
        {
            bool result = GeoTiffInfoReader.TryReadGeoTiffBounds(
                path, out double xMin, out double yMin, out double xMax, out double yMax);

            Assert.That(result, Is.True);
            Assert.That(xMin, Is.EqualTo(-10.0).Within(1e-9));
            Assert.That(xMax, Is.EqualTo(-9.0).Within(1e-9));
            Assert.That(yMin, Is.EqualTo(49.0).Within(1e-9));
            Assert.That(yMax, Is.EqualTo(50.0).Within(1e-9));
        }
        finally { File.Delete(path); }
    }

    [Test]
    public void TryReadGeoTiffBounds_geotiff_without_crs_returns_false()
    {
        // GeoKeyDirectory with zero keys → epsgCode=0 → unknown CRS → returns false
        string path = WriteTempTiff(TiffBuilder.GeoTiffWithPixelScaleAndTiepoint(
            width: 5, height: 5,
            scaleX: 1.0, scaleY: 1.0,
            originX: 10.0, originY: 20.0,
            geoKeyDirectory: new short[] { 1, 1, 0, 0 })); // header only, numKeys=0
        try
        {
            bool result = GeoTiffInfoReader.TryReadGeoTiffBounds(
                path, out _, out _, out _, out _);

            Assert.That(result, Is.False);
        }
        finally { File.Delete(path); }
    }

    [Test]
    public void TryReadGeoTiffBounds_projected_utm_geotiff_reprojects_to_wgs84()
    {
        // UTM Zone 32N (EPSG:32632), area near Frankfurt (~8.8°E, 49.9°N)
        // Origin E=489000, N=5546000; pixel size 1000 m; 10×10 pixels → ~10 km square
        var geoKeys = new short[]
        {
            1, 1, 0, 2,
            1024, 0, 1, 1,      // GTModelTypeGeoKey = 1 (Projected)
            3072, 0, 1, 32632,  // ProjectedCSTypeGeoKey = EPSG:32632 (WGS 84 / UTM zone 32N)
        };
        string path = WriteTempTiff(TiffBuilder.GeoTiffWithPixelScaleAndTiepoint(
            width: 10, height: 10,
            scaleX: 1000.0, scaleY: 1000.0,
            originX: 489_000.0, originY: 5_546_000.0,
            geoKeyDirectory: geoKeys));
        try
        {
            bool result = GeoTiffInfoReader.TryReadGeoTiffBounds(
                path, out double xMin, out double yMin, out double xMax, out double yMax);

            Assert.That(result, Is.True);
            // After reprojection: should be WGS84 degrees near Frankfurt
            Assert.That(xMin, Is.InRange(8.0, 10.0), "xMin should be near 9°E");
            Assert.That(xMax, Is.InRange(8.0, 10.0), "xMax should be near 9°E");
            Assert.That(yMin, Is.InRange(49.0, 51.0), "yMin should be near 50°N");
            Assert.That(yMax, Is.InRange(49.0, 51.0), "yMax should be near 50°N");
            Assert.That(xMax, Is.GreaterThan(xMin));
            Assert.That(yMax, Is.GreaterThan(yMin));
        }
        finally { File.Delete(path); }
    }

    [Test]
    public void TryReadGeoTiffBounds_user_defined_crs_with_esri_pe_wkt_reprojects_to_wgs84()
    {
        // Projected CRS is user-defined (32767); the actual CRS is carried as an ESRI PE String
        // (Web Mercator) in the PCSCitationGeoKey — this mirrors GeoTIFFs produced by ArcGIS.
        const string esriPeWkt =
            "ESRI PE String = PROJCS[\"WGS_1984_Web_Mercator_Auxiliary_Sphere\"," +
            "GEOGCS[\"GCS_WGS_1984\",DATUM[\"D_WGS_1984\"," +
            "SPHEROID[\"WGS_1984\",6378137.0,298.257223563]]," +
            "PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.0174532925199433]]," +
            "PROJECTION[\"Mercator_Auxiliary_Sphere\"],PARAMETER[\"False_Easting\",0.0]," +
            "PARAMETER[\"False_Northing\",0.0],PARAMETER[\"Central_Meridian\",0.0]," +
            "PARAMETER[\"Standard_Parallel_1\",0.0],PARAMETER[\"Auxiliary_Sphere_Type\",0.0]," +
            "UNIT[\"Meter\",1.0]]|";

        var geoKeys = new short[]
        {
            1, 1, 0, 3,
            1024, 0, 1, 1,               // GTModelTypeGeoKey = 1 (Projected)
            3072, 0, 1, 32767,           // ProjectedCSTypeGeoKey = user-defined
            3073, unchecked((short)34737), (short)esriPeWkt.Length, 0, // PCSCitationGeoKey → GeoAsciiParams, offset 0
        };

        // Web Mercator meters near Frankfurt (~8.9°E, 49.9°N)
        string path = WriteTempTiff(TiffBuilder.GeoTiffWithPixelScaleAndTiepoint(
            width: 10, height: 10,
            scaleX: 1000.0, scaleY: 1000.0,
            originX: 989_000.0, originY: 6_430_000.0,
            geoKeyDirectory: geoKeys,
            geoAsciiParams: esriPeWkt));
        try
        {
            bool result = GeoTiffInfoReader.TryReadGeoTiffBounds(
                path, out double xMin, out double yMin, out double xMax, out double yMax);

            Assert.That(result, Is.True);
            Assert.That(xMin, Is.InRange(8.0, 10.0));
            Assert.That(xMax, Is.InRange(8.0, 10.0));
            Assert.That(yMin, Is.InRange(49.0, 51.0));
            Assert.That(yMax, Is.InRange(49.0, 51.0));
            Assert.That(xMax, Is.GreaterThan(xMin));
            Assert.That(yMax, Is.GreaterThan(yMin));
        }
        finally { File.Delete(path); }
    }

    [Test]
    public void TryReadGeoTiffBounds_unresolvable_epsg_code_without_citation_returns_false()
    {
        // A CRS code that DotSpatial cannot resolve and with no citation WKT to fall back on.
        var geoKeys = new short[]
        {
            1, 1, 0, 2,
            1024, 0, 1, 1,      // GTModelTypeGeoKey = 1 (Projected)
            3072, 0, 1, 1,      // ProjectedCSTypeGeoKey = 1 (not a valid EPSG projected code)
        };
        string path = WriteTempTiff(TiffBuilder.GeoTiffWithPixelScaleAndTiepoint(
            width: 5, height: 5,
            scaleX: 1.0, scaleY: 1.0,
            originX: 10.0, originY: 20.0,
            geoKeyDirectory: geoKeys));
        try
        {
            bool result = GeoTiffInfoReader.TryReadGeoTiffBounds(path, out _, out _, out _, out _);
            Assert.That(result, Is.False);
        }
        finally { File.Delete(path); }
    }

    [Test]
    public void TryReadGeoTiffBounds_datum_needs_missing_grid_shift_falls_back_to_bounds()
    {
        // NAD27 (EPSG:4267) uses a grid-shift datum transform to WGS84. DotSpatial ships without
        // the grid files, so the reader must fall back to a grid-free transform instead of failing.
        var geoKeys = new short[]
        {
            1, 1, 0, 2,
            1024, 0, 1, 2,      // GTModelTypeGeoKey = 2 (Geographic)
            2048, 0, 1, 4267,   // GeographicTypeGeoKey = EPSG:4267 (NAD27)
        };
        // Geographic degrees: top-left at (-100°, 40°), pixel size 0.1°, 10×10 pixels
        string path = WriteTempTiff(TiffBuilder.GeoTiffWithPixelScaleAndTiepoint(
            width: 10, height: 10,
            scaleX: 0.1, scaleY: 0.1,
            originX: -100.0, originY: 40.0,
            geoKeyDirectory: geoKeys));
        try
        {
            bool result = GeoTiffInfoReader.TryReadGeoTiffBounds(
                path, out double xMin, out double yMin, out double xMax, out double yMax);

            Assert.That(result, Is.True);
            Assert.That(xMin, Is.EqualTo(-100.0).Within(0.01));
            Assert.That(xMax, Is.EqualTo(-99.0).Within(0.01));
            Assert.That(yMin, Is.EqualTo(39.0).Within(0.01));
            Assert.That(yMax, Is.EqualTo(40.0).Within(0.01));
        }
        finally { File.Delete(path); }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    private static string WriteTempTiff(byte[] bytes)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".tif");
        File.WriteAllBytes(path, bytes);
        return path;
    }

    /// <summary>GeoKey directory encoding WGS84 geographic CRS (EPSG:4326).</summary>
    private static short[] WGS84GeoKeys() => new short[]
    {
        1, 1, 0, 2,       // KeyDirectoryVersion=1, KeyRevision=1, MinorRevision=0, NumKeys=2
        1024, 0, 1, 2,    // GTModelTypeGeoKey = 2 (Geographic)
        2048, 0, 1, 4326, // GeographicTypeGeoKey = EPSG:4326 (WGS84)
    };

    /// <summary>
    /// Builds minimal valid GeoTIFF/TIFF byte arrays (TIFF little-endian, 8-bit grayscale).
    /// Tags are always written in ascending order as required by the TIFF specification.
    /// </summary>
    private static class TiffBuilder
    {
        private const ushort TypeShort  = 3;
        private const ushort TypeLong   = 4;
        private const ushort TypeAscii  = 2;
        private const ushort TypeDouble = 12;

        public static byte[] PlainTiff(int width, int height)
            => Build(width, height, pixelScale: null, tiepoint: null, transformMatrix: null, geoKeyDir: null, geoAsciiParams: null);

        public static byte[] GeoTiffWithCrsOnly(int width, int height, short[] geoKeyDirectory)
            => Build(width, height, pixelScale: null, tiepoint: null, transformMatrix: null, geoKeyDir: geoKeyDirectory, geoAsciiParams: null);

        public static byte[] GeoTiffWithPixelScaleAndTiepoint(int width, int height,
            double scaleX, double scaleY, double originX, double originY, short[] geoKeyDirectory,
            string geoAsciiParams = null)
            => Build(width, height,
                pixelScale: new[] { scaleX, scaleY, 0.0 },
                tiepoint:   new[] { 0.0, 0.0, 0.0, originX, originY, 0.0 },
                transformMatrix: null,
                geoKeyDir: geoKeyDirectory,
                geoAsciiParams: geoAsciiParams);

        public static byte[] GeoTiffWithTransformationMatrix(int width, int height,
            double[] transformMatrix, short[] geoKeyDirectory)
            => Build(width, height, pixelScale: null, tiepoint: null,
                transformMatrix: transformMatrix, geoKeyDir: geoKeyDirectory, geoAsciiParams: null);

        private static byte[] Build(int width, int height,
            double[] pixelScale, double[] tiepoint, double[] transformMatrix, short[] geoKeyDir,
            string geoAsciiParams)
        {
            var entries = new List<(ushort Tag, ushort Type, uint Count, byte[] Data)>();

            void AddShort(ushort tag, ushort value)
                => entries.Add((tag, TypeShort, 1, BitConverter.GetBytes(value)));

            void AddLong(ushort tag, uint value)
                => entries.Add((tag, TypeLong, 1, BitConverter.GetBytes(value)));

            void AddAscii(ushort tag, string value)
            {
                var data = System.Text.Encoding.ASCII.GetBytes(value + "\0");
                entries.Add((tag, TypeAscii, (uint)data.Length, data));
            }

            void AddDoubles(ushort tag, double[] values)
            {
                var data = new byte[values.Length * sizeof(double)];
                for (int i = 0; i < values.Length; i++)
                    BitConverter.GetBytes(values[i]).CopyTo(data, i * sizeof(double));
                entries.Add((tag, TypeDouble, (uint)values.Length, data));
            }

            void AddShorts(ushort tag, short[] values)
            {
                var data = new byte[values.Length * sizeof(short)];
                for (int i = 0; i < values.Length; i++)
                    BitConverter.GetBytes(values[i]).CopyTo(data, i * sizeof(short));
                entries.Add((tag, TypeShort, (uint)values.Length, data));
            }

            // Standard TIFF tags
            AddShort(256, (ushort)width);         // ImageWidth
            AddShort(257, (ushort)height);        // ImageLength
            AddShort(258, 8);                     // BitsPerSample
            AddShort(259, 1);                     // Compression = None
            AddShort(262, 1);                     // PhotometricInterpretation = MinIsBlack
            AddLong(273, 0);                      // StripOffsets — fixed up below
            AddShort(278, (ushort)height);        // RowsPerStrip
            AddLong(279, (uint)(width * height)); // StripByteCounts
            AddShort(284, 1);                     // PlanarConfiguration = Chunky

            // GeoTIFF model tags (optional)
            if (pixelScale != null)    AddDoubles(33550, pixelScale);      // ModelPixelScaleTag
            if (tiepoint != null)      AddDoubles(33922, tiepoint);        // ModelTiepointTag
            if (transformMatrix != null) AddDoubles(34264, transformMatrix); // ModelTransformationTag
            if (geoKeyDir != null)     AddShorts(34735, geoKeyDir);        // GeoKeyDirectoryTag
            if (geoAsciiParams != null) AddAscii(34737, geoAsciiParams);   // GeoAsciiParamsTag

            // IFD entries must be in ascending tag order (TIFF spec §2)
            entries.Sort((a, b) => a.Tag.CompareTo(b.Tag));

            int n           = entries.Count;
            int ifdOffset   = 8;                          // right after header
            int ifdSize     = 2 + n * 12 + 4;            // num(2) + entries + next-IFD(4)
            int pixelOffset = ifdOffset + ifdSize;
            int pixelBytes  = width * height;
            int blobStart   = pixelOffset + pixelBytes;   // where external blobs begin

            // Fix StripOffsets and compute blob positions for large-value entries
            var blobOffsets = new int[n];
            int blobCursor = blobStart;
            for (int i = 0; i < n; i++)
            {
                var (tag, type, count, data) = entries[i];
                if (tag == 273)
                {
                    entries[i] = (tag, TypeLong, 1, BitConverter.GetBytes((uint)pixelOffset));
                }
                else if (data.Length > 4)
                {
                    blobOffsets[i] = blobCursor;
                    blobCursor += data.Length;
                }
            }

            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            // TIFF header (8 bytes)
            bw.Write((byte)'I'); bw.Write((byte)'I'); // II = little-endian
            bw.Write((ushort)42);                     // TIFF magic
            bw.Write((uint)ifdOffset);

            // IFD
            bw.Write((ushort)n);
            for (int i = 0; i < n; i++)
            {
                var (tag, type, count, data) = entries[i];
                bw.Write(tag);
                bw.Write(type);
                bw.Write(count);
                if (data.Length <= 4)
                {
                    bw.Write(data);
                    for (int p = data.Length; p < 4; p++) bw.Write((byte)0); // right-pad
                }
                else
                {
                    bw.Write((uint)blobOffsets[i]);
                }
            }
            bw.Write((uint)0); // next IFD offset = 0 (end)

            // Pixel data (content is irrelevant for georeferencing tests)
            bw.Write(new byte[pixelBytes]);

            // External data blobs, in the same order as entries
            for (int i = 0; i < n; i++)
            {
                var (tag, _, _, data) = entries[i];
                if (tag != 273 && data.Length > 4)
                    bw.Write(data);
            }

            bw.Flush();
            return ms.ToArray();
        }
    }
}
