using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BitMiracle.LibTiff.Classic;
using DotSpatial.Projections;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services;

public static class GeoTiffInfoReader
{
    public static bool IsGeoTIFF(string geoTiffFilePath)
    {
        using Tiff tiff = Tiff.Open(geoTiffFilePath, "r");
        return IsGeoTiff(tiff);
    }
    
    public static bool IsGeoTIFF(byte[] geotiffBytes)
    {
        using MemoryStream ms = new MemoryStream(geotiffBytes);
        using Tiff tiff = Tiff.ClientOpen("in-memory", "r", ms, new TiffStream());
        return IsGeoTiff(tiff);
    }

    private static bool IsGeoTiff(Tiff tiff)
    {
        var geoKeyDirectoryTag = tiff?.GetField(TiffTag.GEOTIFF_GEOKEYDIRECTORYTAG);
        tiff?.Close();
        return geoKeyDirectoryTag != null;
    }

    /// <summary>
    /// Reads the WGS84 bounding box from a GeoTIFF file using only managed .NET code (no external GDAL tool required).
    /// Returns false when the file has no georeferencing tags or reprojection fails.
    /// </summary>
    public static bool TryReadGeoTiffBounds(string filePath,
        out double xMin, out double yMin, out double xMax, out double yMax)
    {
        xMin = xMax = yMin = yMax = 0;

        using Tiff tiff = Tiff.Open(filePath, "r");
        if (tiff == null)
            return false;

        var widthField = tiff.GetField(TiffTag.IMAGEWIDTH);
        var heightField = tiff.GetField(TiffTag.IMAGELENGTH);
        if (widthField == null || heightField == null)
            return false;

        int width = widthField[0].ToInt();
        int height = heightField[0].ToInt();

        // Affine transform coefficients: X = a*col + b*row + tx; Y = c*col + d*row + ty
        double a, b, c, d, tx, ty;

        var transformField = tiff.GetField(TiffTag.GEOTIFF_MODELTRANSFORMATIONTAG);
        if (transformField != null)
        {
            var values = ReadDoubleArray(transformField);
            if (values == null || values.Length < 16)
                return false;

            // Row-major 4×4 affine matrix: [a b 0 tx | c d 0 ty | ...]
            a = values[0]; b = values[1]; tx = values[3];
            c = values[4]; d = values[5]; ty = values[7];
        }
        else
        {
            var pixelScaleField = tiff.GetField(TiffTag.GEOTIFF_MODELPIXELSCALETAG);
            var tiepointField = tiff.GetField(TiffTag.GEOTIFF_MODELTIEPOINTTAG);

            if (pixelScaleField == null || tiepointField == null)
                return false;

            var scales = ReadDoubleArray(pixelScaleField);
            var tiepoints = ReadDoubleArray(tiepointField);

            if (scales == null || scales.Length < 2 || tiepoints == null || tiepoints.Length < 6)
                return false;

            double sx = scales[0]; // positive pixel size in X (eastward)
            double sy = scales[1]; // positive pixel size in Y (northward, applied inverted per row)

            // First tiepoint maps pixel (I, J) → world (X, Y)
            double pixelI = tiepoints[0];
            double pixelJ = tiepoints[1];
            double worldX = tiepoints[3];
            double worldY = tiepoints[4];

            tx = worldX - pixelI * sx;
            ty = worldY + pixelJ * sy;
            a = sx; b = 0;
            c = 0;  d = -sy; // Y decreases as pixel row increases in north-up rasters
        }

        // Transform the four image corners from pixel space to native CRS
        var (ulX, ulY) = (tx, ty);
        var (urX, urY) = (a * width + tx,          c * width + ty);
        var (llX, llY) = (b * height + tx,          d * height + ty);
        var (lrX, lrY) = (a * width + b * height + tx, c * width + d * height + ty);

        // Resolve the source coordinate reference system and reproject to WGS84 when needed
        if (!TryResolveSourceProjection(tiff, out ProjectionInfo source, out bool alreadyWgs84))
            return false; // CRS could not be resolved – cannot guarantee WGS84 output

        if (!alreadyWgs84)
        {
            try
            {
                var wgs84 = KnownCoordinateSystems.Geographic.World.WGS1984;

                // DotSpatial ships without NADCON/NTv2 grid-shift files. If the source datum needs
                // them, drop the grid-based datum shift up front – the sub-metre difference is
                // irrelevant for a map bounding box and keeps otherwise valid maps usable.
                NeutralizeMissingGridShift(source);

                (ulX, ulY) = ReprojectPoint(ulX, ulY, source, wgs84);
                (urX, urY) = ReprojectPoint(urX, urY, source, wgs84);
                (llX, llY) = ReprojectPoint(llX, llY, source, wgs84);
                (lrX, lrY) = ReprojectPoint(lrX, lrY, source, wgs84);
            }
            catch (Exception)
            {
                return false;
            }
        }

        xMin = Math.Min(Math.Min(ulX, urX), Math.Min(llX, lrX));
        xMax = Math.Max(Math.Max(ulX, urX), Math.Max(llX, lrX));
        yMin = Math.Min(Math.Min(ulY, urY), Math.Min(llY, lrY));
        yMax = Math.Max(Math.Max(ulY, urY), Math.Max(llY, lrY));

        return true;
    }

    private static (double X, double Y) ReprojectPoint(double x, double y, ProjectionInfo source, ProjectionInfo target)
    {
        var coords = new double[] { x, y };
        try
        {
            Reproject.ReprojectPoints(coords, new double[] { 0 }, source, target, 0, 1);
        }
        catch (GridShiftMissingException)
        {
            // Grid-shift file is unavailable – retry without the grid-based datum transform.
            NeutralizeMissingGridShift(source);
            coords = new double[] { x, y };
        Reproject.ReprojectPoints(coords, new double[] { 0 }, source, target, 0, 1);
        }
        return (coords[0], coords[1]);
    }

    private static void NeutralizeMissingGridShift(ProjectionInfo projection)
    {
        var datum = projection.GeographicInfo?.Datum;
        if (datum != null && datum.DatumType == DatumType.GridShift)
            datum.DatumType = DatumType.WGS84;
    }

    private static double[] ReadDoubleArray(FieldValue[] field)
    {
        if (field == null || field.Length < 2) return null;

        var value = field[1].Value;
        if (value is double[] doubles)
            return doubles;

        if (value is byte[] bytes)
        {
            int count = bytes.Length / sizeof(double);
            var result = new double[count];
            for (int i = 0; i < count; i++)
                result[i] = BitConverter.ToDouble(bytes, i * sizeof(double));
            return result;
        }

        return null;
    }

    private const int UserDefinedGeoKey = 32767;

    // GeoTIFF GeoKey identifiers (see the GeoTIFF specification, Appendix A).
    private const int GTModelTypeGeoKey = 1024;      // 1=Projected, 2=Geographic, 3=Geocentric
    private const int GTCitationGeoKey = 1026;       // ASCII, may hold ESRI PE WKT
    private const int GeographicTypeGeoKey = 2048;   // EPSG geographic CRS code
    private const int GeogCitationGeoKey = 2049;     // ASCII, may hold ESRI PE WKT
    private const int ProjectedCSTypeGeoKey = 3072;  // EPSG projected CRS code
    private const int PCSCitationGeoKey = 3073;      // ASCII, may hold ESRI PE WKT

    private const int GeoAsciiParamsTagLocation = 34737;

    // Well-known vendor / deprecated CRS codes that DotSpatial cannot resolve numerically,
    // mapped to their EPSG equivalents (all variants below are Web Mercator).
    private static readonly Dictionary<int, int> VendorCrsToEpsg = new()
    {
        { 102100, 3857 },
        { 102113, 3857 },
        { 900913, 3857 },
        { 3785, 3857 },
        { 41001, 3857 },
    };

    /// <summary>
    /// Resolves the source projection of a GeoTIFF using several strategies, mirroring the resilience of GDAL:
    /// direct EPSG code, vendor (ESRI) code mapping, and ESRI WKT parsed from the citation GeoKeys.
    /// </summary>
    private static bool TryResolveSourceProjection(Tiff tiff, out ProjectionInfo source, out bool isWgs84)
    {
        source = null;
        isWgs84 = false;

        var entries = ReadGeoKeyEntries(tiff);
        if (entries.Count == 0)
            return false;

        var asciiParams = ReadGeoAsciiParams(tiff);

        int modelType = ReadShortGeoKey(entries, GTModelTypeGeoKey) ?? 0;
        int geographicCode = ReadShortGeoKey(entries, GeographicTypeGeoKey) ?? 0;
        int projectedCode = ReadShortGeoKey(entries, ProjectedCSTypeGeoKey) ?? 0;

        // Projected CRS takes priority over the underlying geographic CRS.
        int primaryCode =
            (projectedCode != 0 && projectedCode != UserDefinedGeoKey) ? projectedCode :
            (geographicCode != 0 && geographicCode != UserDefinedGeoKey) ? geographicCode : 0;

        // Fast path: geographic WGS84 needs no reprojection.
        if (modelType == 2 && primaryCode == 4326)
        {
            source = KnownCoordinateSystems.Geographic.World.WGS1984;
            isWgs84 = true;
            return true;
        }

        // 1. Direct EPSG resolution.
        if (primaryCode != 0 && TryFromEpsg(primaryCode, out source))
            return true;

        // 2. Known vendor / deprecated code mapped to EPSG.
        if (primaryCode != 0
            && VendorCrsToEpsg.TryGetValue(primaryCode, out int mappedEpsg)
            && TryFromEpsg(mappedEpsg, out source))
            return true;

        // 3. ESRI WKT embedded in the citation GeoKeys (common for ArcGIS user-defined CRS).
        foreach (int citationKey in new[] { PCSCitationGeoKey, GTCitationGeoKey, GeogCitationGeoKey })
        {
            var citation = ReadAsciiGeoKey(entries, asciiParams, citationKey);
            if (TryFromEsriWkt(citation, out source))
                return true;
        }

        return false;
    }

    private static bool TryFromEpsg(int code, out ProjectionInfo projection)
    {
        projection = null;
        try
        {
            projection = ProjectionInfo.FromEpsgCode(code);
            return projection != null;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static bool TryFromEsriWkt(string citation, out ProjectionInfo projection)
    {
        projection = null;
        if (string.IsNullOrWhiteSpace(citation))
            return false;

        var wkt = citation.Trim();

        // ArcGIS prefixes the embedded WKT, e.g. "ESRI PE String = PROJCS[...]".
        const string esriPePrefix = "ESRI PE String = ";
        int prefixIndex = wkt.IndexOf(esriPePrefix, StringComparison.OrdinalIgnoreCase);
        if (prefixIndex >= 0)
            wkt = wkt.Substring(prefixIndex + esriPePrefix.Length).Trim();

        if (!(wkt.Contains("PROJCS[") || wkt.Contains("GEOGCS[")
              || wkt.Contains("PROJCRS[") || wkt.Contains("GEOGCRS[")))
            return false;

        try
        {
            projection = ProjectionInfo.FromEsriString(wkt);
            return projection != null;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static int? ReadShortGeoKey(List<GeoKeyEntry> entries, int keyId)
    {
        foreach (var entry in entries)
        {
            // Inline SHORT values are stored directly in the value/offset slot (TIFFTagLocation == 0).
            if (entry.KeyId == keyId && entry.TiffTagLocation == 0)
                return entry.ValueOffset;
        }

        return null;
    }

    private static string ReadAsciiGeoKey(List<GeoKeyEntry> entries, string asciiParams, int keyId)
    {
        if (string.IsNullOrEmpty(asciiParams))
            return null;

        foreach (var entry in entries)
        {
            if (entry.KeyId != keyId || entry.TiffTagLocation != GeoAsciiParamsTagLocation)
                continue;

            int offset = entry.ValueOffset;
            int count = entry.Count;
            if (offset < 0 || offset >= asciiParams.Length)
                return null;

            if (count <= 0 || offset + count > asciiParams.Length)
                count = asciiParams.Length - offset;

            // The GeoTIFF spec terminates each ASCII value with '|' (a stand-in for NUL).
            return asciiParams.Substring(offset, count).TrimEnd('|', '\0', ' ');
        }

        return null;
    }

    private static string ReadGeoAsciiParams(Tiff tiff)
    {
        var field = tiff.GetField(TiffTag.GEOTIFF_GEOASCIIPARAMSTAG);
        if (field == null || field.Length < 2)
            return null;

        var value = field[1].Value;
        return value switch
        {
            string s => s,
            byte[] bytes => Encoding.ASCII.GetString(bytes),
            _ => value?.ToString()
        };
    }

    private static List<GeoKeyEntry> ReadGeoKeyEntries(Tiff tiff)
    {
        var result = new List<GeoKeyEntry>();

        var geoKeyDir = tiff.GetField(TiffTag.GEOTIFF_GEOKEYDIRECTORYTAG);
        if (geoKeyDir == null || geoKeyDir.Length < 2)
            return result;

        short[] shorts;
        var value = geoKeyDir[1].Value;
        if (value is short[] s)
        {
            shorts = s;
        }
        else if (value is byte[] bytes)
        {
            shorts = new short[bytes.Length / sizeof(short)];
            for (int i = 0; i < shorts.Length; i++)
                shorts[i] = BitConverter.ToInt16(bytes, i * sizeof(short));
        }
        else
        {
            return result;
        }

        if (shorts.Length < 4)
            return result;

        int numKeys = shorts[3];
        for (int i = 0; i < numKeys; i++)
        {
            int offset = 4 + i * 4;
            if (offset + 3 >= shorts.Length) break;

            result.Add(new GeoKeyEntry(
                keyId: (ushort)shorts[offset],
                tiffTagLocation: (ushort)shorts[offset + 1],
                count: (ushort)shorts[offset + 2],
                valueOffset: (ushort)shorts[offset + 3])); // cast to ushort to handle codes > 32767
        }

        return result;
    }

    private readonly struct GeoKeyEntry
    {
        public GeoKeyEntry(int keyId, int tiffTagLocation, int count, int valueOffset)
        {
            KeyId = keyId;
            TiffTagLocation = tiffTagLocation;
            Count = count;
            ValueOffset = valueOffset;
        }

        public int KeyId { get; }
        public int TiffTagLocation { get; }
        public int Count { get; }
        public int ValueOffset { get; }
    }
}
