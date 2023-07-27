using System;
using DotSpatial.Projections;
using NetTopologySuite.Geometries;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services;

public class CoordinateTransformationFilter : ICoordinateSequenceFilter
{
    private readonly ProjectionInfo source;
    private readonly ProjectionInfo target;

    public CoordinateTransformationFilter(ProjectionInfo source, ProjectionInfo target)
    {
        this.source = source;
        this.target = target;
    }

    public Coordinate Transform(double x, double y)
    {
        var coordinate = new double[] { x, y };
        Reproject.ReprojectPoints(coordinate, Array.Empty<double>(), source, target, 0, 1);
        return new Coordinate(coordinate[0], coordinate[1]);
    }

    public void Filter(CoordinateSequence seq, int idx)
    {
        double[] points = new double[seq.Count * 2];
        double[] z = new double[seq.Count * 2];

        for (int i = 0; i < seq.Count; i++)
        {
            var geomCoordinate = seq.GetCoordinate(i);
            points[i * 2] = geomCoordinate.X;
            points[i * 2 + 1] = geomCoordinate.Y;
            z[i] = geomCoordinate.Z;
        }

        Reproject.ReprojectPoints(points, z, source, target, 0, seq.Count);
        
        for (int i = 0; i < seq.Count; i++)
        {
            var geomCoordinate = seq.GetCoordinate(i);
            geomCoordinate.X = points[i * 2];
            geomCoordinate.Y = points[i * 2 + 1];
        }
    }

    public bool Done => true;
    public bool GeometryChanged => true;
}
