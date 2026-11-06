using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Clustering;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Clustering
{
    public class SuperClusterTests
    {
        private static Feature Point(double lat, double lng) =>
            new Feature(new Point(new Position(lat, lng)), new Dictionary<string, object>());

        private static SuperCluster Load(params Feature[] features)
        {
            var cluster = new SuperCluster();
            cluster.Load(features);
            return cluster;
        }

        [Test]
        public void should_merge_close_points_at_low_zoom_and_split_at_high_zoom()
        {
            var cluster = Load(Point(10, 10), Point(10.0001, 10.0001), Point(-40, 100));
            var world = new GeoBounds(-90, -180, 90, 180);

            var low = cluster.GetClusters(world, 5);
            low.Should().HaveCount(2);
            low.Select(p => p.UserData.NumPoints).Should().BeEquivalentTo(new[] { 2, 1 });

            cluster.GetClusters(world, 17).Should().HaveCount(3);
        }

        [Test]
        public void should_return_only_points_inside_bounds()
        {
            var cluster = Load(Point(10, 10), Point(-40, 100));

            var result = cluster.GetClusters(new GeoBounds(0, 0, 20, 20), 10);

            result.Should().ContainSingle();
            result[0].Latitude.Should().BeApproximately(10, 1e-6);
            result[0].Longitude.Should().BeApproximately(10, 1e-6);
        }

        [Test]
        public void should_return_zoom_at_which_cluster_splits()
        {
            var cluster = Load(Point(10, 10), Point(10.0001, 10.0001));

            var clusterId = cluster.GetClusters(new GeoBounds(-90, -180, 90, 180), 3).Single().UserData.Index;

            var zoom = cluster.GetClusterExpansionZoom(clusterId);

            cluster.GetClusters(new GeoBounds(-90, -180, 90, 180), zoom).Should().HaveCount(2);
            cluster.GetClusters(new GeoBounds(-90, -180, 90, 180), zoom - 1).Should().HaveCount(1);
        }

        [Test]
        public void should_expand_bounds_to_cover_all_points()
        {
            var bounds = GeoBounds.Closed.Expand(10, 20).Expand(-5, 30).Expand(15, 25);

            bounds.South.Should().Be(-5);
            bounds.North.Should().Be(15);
            bounds.West.Should().Be(20);
            bounds.East.Should().Be(30);
        }
    }
}
