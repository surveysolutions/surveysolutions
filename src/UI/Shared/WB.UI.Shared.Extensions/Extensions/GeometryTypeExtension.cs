using System;
using Esri.ArcGISRuntime.Geometry;

namespace WB.UI.Shared.Extensions.Extensions;

public static class GeometryTypeExtension
{
    public static GeometryType ToEsriGeometryType(this WB.Core.SharedKernels.Questionnaire.Documents.GeometryType geometryType)
    {
        switch (geometryType)
        {
            case Core.SharedKernels.Questionnaire.Documents.GeometryType.Multipoint:
                return GeometryType.Multipoint;
            case Core.SharedKernels.Questionnaire.Documents.GeometryType.Point:
                return GeometryType.Point;
            case Core.SharedKernels.Questionnaire.Documents.GeometryType.Polygon:
                return GeometryType.Polygon;
            case Core.SharedKernels.Questionnaire.Documents.GeometryType.Polyline:
                return GeometryType.Polyline;
            default:
                throw new Exception("Unknown geometry type: " + geometryType);
        }
    }
}