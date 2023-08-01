using System;
using DotSpatial.Projections;
using NetTopologySuite.Geometries;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services;

public class CoordinateTransformationFilter : ICoordinateFilter
{
    private readonly ProjectionInfo source;
    private readonly ProjectionInfo target;

    public CoordinateTransformationFilter(ProjectionInfo source, ProjectionInfo target)
    {
        this.source = source;
        this.target = target;
    }

    public void Filter(Coordinate coord)
    {
        var coordinate = new double[] { coord.X, coord.Y };
        Reproject.ReprojectPoints(coordinate, new double[] { coord.Z }, source, target, 0, 1);
        coord.X = coordinate[0];
        coord.Y = coordinate[1];
    }
            
    public Coordinate Transform(double x, double y)
    {
        var coordinate = new double[] { x, y };
        Reproject.ReprojectPoints(coordinate, Array.Empty<double>(), source, target, 0, 1);
        return new Coordinate(coordinate[0], coordinate[1]);
    }
}
