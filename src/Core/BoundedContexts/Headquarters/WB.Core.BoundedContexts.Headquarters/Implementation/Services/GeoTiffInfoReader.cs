using System;
using System.IO;
using BitMiracle.LibTiff.Classic;
using DotSpatial.Projections;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services;

public class GeoTiffInfoReader
{
    public class Wgs84Extent
    {
        public double MinX { get; set; }
        public double MaxX { get; set; }
        public double MinY { get; set; }
        public double MaxY { get; set; }
    }
    
    public bool TryReadGeotiffCoordinates(string geoTiffFilePath, out Wgs84Extent wgs84Extent)
    {
        wgs84Extent = null;

        using var tiff = Tiff.Open(geoTiffFilePath, "r");
        if (tiff == null)
            return false;

        var modelPixelScaleTag = tiff.GetField(TiffTag.GEOTIFF_MODELPIXELSCALETAG);
        if (modelPixelScaleTag != null)
        {
            double scaleX = BitConverter.ToDouble(modelPixelScaleTag[1].GetBytes(), 0);
            double scaleY = BitConverter.ToDouble(modelPixelScaleTag[1].GetBytes(), 8);

            var modelTiepointTag = tiff.GetField(TiffTag.GEOTIFF_MODELTIEPOINTTAG);
            if (modelTiepointTag != null)
            {
                double originX = BitConverter.ToDouble(modelTiepointTag[1].GetBytes(), 24);
                double originY = BitConverter.ToDouble(modelTiepointTag[1].GetBytes(), 32);

                double minX = originX;
                double minY = originY;
                double maxX = originX + scaleX * tiff.GetField(TiffTag.IMAGEWIDTH)[0].ToDouble();
                double maxY = originY + scaleY * tiff.GetField(TiffTag.IMAGELENGTH)[0].ToDouble();

                ProjectionInfo source = null;
                    
                var geoKeyDirectoryTag = tiff.GetField(TiffTag.GEOTIFF_GEOKEYDIRECTORYTAG);
                var geoKeyDirectory = geoKeyDirectoryTag[1].ToUShortArray();

                int projectedCSTypeGeoKey = 0;
                int geographicTypeGeoKey = 0;

                for (var keyIndex = 0; keyIndex < geoKeyDirectory.Length; keyIndex += 4)
                {
                    ushort key = geoKeyDirectory[keyIndex];
                    ushort value = geoKeyDirectory[keyIndex + 3];

                    if (key == 3072) // ProjectedCSTypeGeoKey
                    {
                        projectedCSTypeGeoKey = value;
                    }
                    else if (key == 2048) // GeographicTypeGeoKey
                    {
                        geographicTypeGeoKey = value;
                    }
                }

                if (projectedCSTypeGeoKey != 0)
                {
                    source = ProjectionInfo.FromEpsgCode(projectedCSTypeGeoKey);
                }
                else if (geographicTypeGeoKey != 0)
                {
                    source = ProjectionInfo.FromEpsgCode(geographicTypeGeoKey);
                }
                else
                {
                    return false;
                }

                if (source != null)
                {
                    var target = KnownCoordinateSystems.Geographic.World.WGS1984;
                    var coordinate = new double[] { minX, minY, maxX, maxY };
                    Reproject.ReprojectPoints(coordinate, Array.Empty<double>(), source, target, 0, 2);
                    
                    minX = coordinate[0];
                    minY = coordinate[1];
                    maxX = coordinate[2];
                    maxY = coordinate[3];
                }

                wgs84Extent = new Wgs84Extent
                {
                    MinX = minX,
                    MinY = minY,
                    MaxX = maxX,
                    MaxY = maxY
                };
                
                return true;
            }
        }

        return false;
    }
    

    public bool IsGeoTIFF(string geoTiffFilePath)
    {
        using var tiff = Tiff.Open(geoTiffFilePath, "r");
        tiff.Close();
        
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
