using System;
using System.IO;
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

        // Reproject to WGS84 when needed
        var (epsgCode, modelType) = ReadEpsgFromGeoKeys(tiff);

        bool alreadyWgs84 = modelType == 2 && epsgCode == 4326;
        bool unknownCrs = epsgCode == 0;

        if (!alreadyWgs84 && !unknownCrs)
        {
            try
            {
                var source = ProjectionInfo.FromEpsgCode(epsgCode);
                var wgs84 = KnownCoordinateSystems.Geographic.World.WGS1984;

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
        Reproject.ReprojectPoints(coords, new double[] { 0 }, source, target, 0, 1);
        return (coords[0], coords[1]);
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

    private static (int EpsgCode, int ModelType) ReadEpsgFromGeoKeys(Tiff tiff)
    {
        var geoKeyDir = tiff.GetField(TiffTag.GEOTIFF_GEOKEYDIRECTORYTAG);
        if (geoKeyDir == null)
            return (0, 0);

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
            return (0, 0);
        }

        if (shorts.Length < 4)
            return (0, 0);

        int numKeys = shorts[3];
        int modelType = 0;
        int epsgCode = 0;

        for (int i = 0; i < numKeys; i++)
        {
            int offset = 4 + i * 4;
            if (offset + 3 >= shorts.Length) break;

            int keyId = shorts[offset];
            int tiffTagLocation = shorts[offset + 1];
            int valueOffset = (ushort)shorts[offset + 3]; // cast to ushort to handle codes > 32767

            if (tiffTagLocation != 0) continue; // only inline SHORT values carry EPSG codes

            switch (keyId)
            {
                case 1024: // GTModelTypeGeoKey: 1=Projected, 2=Geographic, 3=Geocentric
                    modelType = valueOffset;
                    break;
                case 2048: // GeographicTypeGeoKey: EPSG geographic CRS
                    if (epsgCode == 0)
                        epsgCode = valueOffset;
                    break;
                case 3072: // ProjectedCSTypeGeoKey: EPSG projected CRS (takes priority)
                    epsgCode = valueOffset;
                    break;
            }
        }

        return (epsgCode, modelType);
    }
}
