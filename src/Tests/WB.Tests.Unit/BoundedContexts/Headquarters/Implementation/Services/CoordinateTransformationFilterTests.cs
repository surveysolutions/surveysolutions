using DotSpatial.Projections;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Implementation.Services;

[TestOf(typeof(CoordinateTransformationFilter))]
public class CoordinateTransformationFilterTests
{
    [Test]
    public void when_transform_coordinate()
    {
        var source = KnownCoordinateSystems.Projected.World.Mercatorworld;
        var target = KnownCoordinateSystems.Geographic.World.WGS1984;
        var filter = new CoordinateTransformationFilter(source, target);

        var coordinate = filter.Transform(7.777, 9.999);
        
        Assert.That(coordinate.X, Is.EqualTo(6.9861979645975182E-05d));
        Assert.That(coordinate.Y, Is.EqualTo(9.0427876878547455E-05d));
    }
}
