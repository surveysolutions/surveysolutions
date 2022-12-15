using System.Globalization;
using System.Linq;
using Esri.ArcGISRuntime.Geometry;
using Java.Security;

namespace WB.UI.Shared.Extensions.ViewModels;

public class GeometryHelper
{
    public static int GetGeometryPointsCount(Geometry geometry)
    {
        if (geometry == null)
            return 0; 
        
        switch (geometry.GeometryType)
        {
            case Esri.ArcGISRuntime.Geometry.GeometryType.Point:
                return geometry.IsEmpty ? 0 : 1;

            case Esri.ArcGISRuntime.Geometry.GeometryType.Polyline:
                return ((Polyline) geometry).Parts.Sum(p => p.PointCount);

            case Esri.ArcGISRuntime.Geometry.GeometryType.Polygon:
                return ((Polygon) geometry).Parts.Sum(p => p.PointCount);

            case Esri.ArcGISRuntime.Geometry.GeometryType.Multipoint:
                return ((Multipoint) geometry).Points.Count();
            default:
                return 0;
        }
    }
    
    public static bool DoesGeometrySupportAreaCalculation(Geometry geometry)
    {
        if (geometry == null)
            return false;

        if (geometry.GeometryType != Esri.ArcGISRuntime.Geometry.GeometryType.Polygon || geometry.Dimension != GeometryDimension.Area)
            return false;

        var polygon = geometry as Polygon;
        if (polygon == null)
            return false;

        if (polygon.Parts.Count < 1)
            return false;

        var readOnlyPart = polygon.Parts.SelectMany(p => p.Points).ToList();
        if (readOnlyPart.Count < 3)
            return false;

        var groupedPoints = from point in readOnlyPart
            group point by new { X = point.X, Y = point.Y } into xyPoint
            select new { X = xyPoint.Key.X, Y = xyPoint.Key.Y, Count = xyPoint.Count() };

        if (groupedPoints.Count() < 3)
            return false;

        return true;
    }
    
    public static string GetProjectedCoordinates(Geometry result, out int count)
    {
        count = 0;
        string coordinates = string.Empty;
        if(result == null)
            return coordinates;
            
        //project to geo-coordinates
        SpatialReference reference = SpatialReference.Create(4326);

        switch (result.GeometryType)
        {
            case Esri.ArcGISRuntime.Geometry.GeometryType.Polygon:
                var polygonCoordinates = ((Polygon) result).Parts.SelectMany(p => p.Points)
                    .Select(point => GeometryEngine.Project(point, reference) as MapPoint)
                    .Select(coordinate => 
                        $"{coordinate.X.ToString(CultureInfo.InvariantCulture)},{coordinate.Y.ToString(CultureInfo.InvariantCulture)}").ToList();
                count = polygonCoordinates.Count;
                return string.Join(";", polygonCoordinates);
            case Esri.ArcGISRuntime.Geometry.GeometryType.Point:
                var projected = GeometryEngine.Project(result as MapPoint, reference) as MapPoint;
                count = 1;
                return $"{projected.X.ToString(CultureInfo.InvariantCulture)},{projected.Y.ToString(CultureInfo.InvariantCulture)}";
            case Esri.ArcGISRuntime.Geometry.GeometryType.Polyline:
                var polylineCoordinates = ((Polyline) result).Parts.SelectMany(p => p.Points)
                    .Select(point => GeometryEngine.Project(point, reference) as MapPoint)
                    .Select(coordinate => 
                        $"{coordinate.X.ToString(CultureInfo.InvariantCulture)},{coordinate.Y.ToString(CultureInfo.InvariantCulture)}").ToList();
                count = polylineCoordinates.Count;
                return string.Join(";", polylineCoordinates);
            case Esri.ArcGISRuntime.Geometry.GeometryType.Multipoint:
                var projectedMultipoint = (GeometryEngine.Project(result as Multipoint, reference) as Multipoint)
                    .Points.Select(coordinate =>
                        $"{coordinate.X.ToString(CultureInfo.InvariantCulture)},{coordinate.Y.ToString(CultureInfo.InvariantCulture)}")
                    .ToList();
                count = projectedMultipoint.Count;
                return string.Join(";", projectedMultipoint);
        }

        throw new InvalidParameterException("Invalid geometry type");
    }
    
    public static double GetGeometryArea(Geometry geometry)
    {
        bool doesGeometrySupportDimensionsCalculation = DoesGeometrySupportAreaCalculation(geometry);
        return doesGeometrySupportDimensionsCalculation ? GeometryEngine.AreaGeodetic(geometry) : 0;
    }

    public static double GetGeometryLength(Geometry geometry)
    {
        bool doesGeometrySupportDimensionsCalculation = DoesGeometrySupportLengthCalculation(geometry);
        return doesGeometrySupportDimensionsCalculation ? GeometryEngine.LengthGeodetic(geometry) : 0;
    }
        
    public static bool DoesGeometrySupportLengthCalculation(Geometry geometry)
    {
        if (geometry == null)
            return false;

        return geometry.GeometryType != GeometryType.Multipoint && geometry.GeometryType != GeometryType.Point;
    }
}
