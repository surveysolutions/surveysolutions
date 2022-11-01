using System;
using Esri.ArcGISRuntime.Geometry;
using GeometryType = WB.Core.SharedKernels.Questionnaire.Documents.GeometryType;

namespace WB.UI.Shared.Extensions.ViewModels;

internal class GeometryByTypeBuilder
{
    private PolylineBuilder polylineBuilder; 
    private PolygonBuilder polygonBuilder;
    private MultipointBuilder multipointBuilder;
    private MapPointBuilder mapPointBuilder;

    private GeometryType geometryType;
        
        
    public GeometryByTypeBuilder(SpatialReference spatialReference, GeometryType geometryType)
    {
        this.geometryType = geometryType;
            
        switch (geometryType)
        {
            case GeometryType.Polygon:
                polygonBuilder = new PolygonBuilder(spatialReference);
                break;
            case GeometryType.Polyline:
                polylineBuilder = new PolylineBuilder(spatialReference);
                break;
            case GeometryType.Point:
                mapPointBuilder = new MapPointBuilder(spatialReference);
                break;
            case GeometryType.Multipoint:
                multipointBuilder = new MultipointBuilder(spatialReference);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(geometryType), geometryType, "Invalid requested type");
        }
            
    }

    public Geometry ToGeometry()
    {
        return geometryType switch
        {
            GeometryType.Polygon => polygonBuilder.ToGeometry(),
            GeometryType.Polyline => polylineBuilder.ToGeometry(),
            GeometryType.Point => mapPointBuilder.ToGeometry(),
            GeometryType.Multipoint => multipointBuilder.ToGeometry(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public SpatialReference SpatialReference {
        get
        {
            return geometryType switch
            {
                GeometryType.Polygon => polygonBuilder.SpatialReference,
                GeometryType.Polyline => polylineBuilder.SpatialReference,
                GeometryType.Point => mapPointBuilder.SpatialReference,
                GeometryType.Multipoint => multipointBuilder.SpatialReference,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    public int PointCount {
        get
        {
            return geometryType switch
            {
                GeometryType.Polygon => polygonBuilder.Parts[0].PointCount,
                GeometryType.Polyline => polylineBuilder.Parts[0].PointCount,
                GeometryType.Point => mapPointBuilder.IsEmpty ? 0 : 1,
                GeometryType.Multipoint => multipointBuilder.Points.Count,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    public void AddPoint(MapPoint point)
    {
        switch (geometryType)
        {
            case GeometryType.Polygon:
                polygonBuilder.AddPoint(point);
                break;
            case GeometryType.Polyline:
                polylineBuilder.AddPoint(point);
                break;
            case GeometryType.Point:
                mapPointBuilder.ReplaceGeometry(point);
                break;
            case GeometryType.Multipoint:
                multipointBuilder.Points.Add(point);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(geometryType), geometryType, "Invalid requested type");
        }
    }
}
