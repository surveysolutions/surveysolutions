using System.IO;
using BitMiracle.LibTiff.Classic;

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
}
