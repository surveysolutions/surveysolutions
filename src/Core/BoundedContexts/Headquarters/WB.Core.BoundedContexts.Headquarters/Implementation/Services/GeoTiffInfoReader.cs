using System.IO;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services;

public class GeoTiffInfoReader
{
    public bool IsGeoTIFF(string geoTiffFilePath)
    {
        byte[] geotiffBytes = File.ReadAllBytes(geoTiffFilePath);
        return IsGeoTIFF(geotiffBytes);
    }
    
    public bool IsGeoTIFF(byte[] geotiffBytes)
    {
        if (geotiffBytes.Length < 4)
            return false;

        // Check the file signature to verify if it is a GeoTIFF file.
        if (geotiffBytes[0] == 0x49 && // I
            geotiffBytes[1] == 0x49 && // I
            geotiffBytes[2] == 0x2A)   // *
        {
            return true;
        }

        if (geotiffBytes[0] == 0x4D && // M
            geotiffBytes[1] == 0x4D && // M
            geotiffBytes[2] == 0x2A)   // *
        {
            return true;
        }

        return false;
    }
}
