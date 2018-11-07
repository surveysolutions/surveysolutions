using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeoJSON.Net.Feature;
using Newtonsoft.Json;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Clustering;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Clustering
{
    /// <summary>
    /// Tests are ported from https://github.com/mapbox/supercluster/blob/v4.1.1/test/test.js
    /// </summary>
    public class SuperClusterTests
    {
        private List<Feature> geoPoints;

        [OneTimeSetUp]
        public void Context()
        {
            var assembly = GetType().Assembly;
            var resourceName = GetType().Namespace + ".places.json";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                var places = JsonConvert.DeserializeObject<FeatureCollection>(reader.ReadToEnd());
                geoPoints = places.Features;
            }
        }

        /// <summary>
        /// Default options that will ensure all expected values are same as in
        /// https://github.com/mapbox/supercluster/blob/v4.1.1/test/test.js
        /// </summary>
        private SuperCluster.SuperClusterOptions DefaultOptions
            => new SuperCluster.SuperClusterOptions
            {
                Radius = 40,
                Extent = 512,
                NodeSize = 64,
                MinZoom = 0,
                MaxZoom = 16
            };

        [Test]
        public void CanGetChildren()
        {
            var cluster = new SuperCluster(DefaultOptions);
            cluster.Load(geoPoints);

            var childCounts = cluster.GetChildren(1);
            Assert.That(childCounts.Select(c => c.UserData.NumPoints), Is.EqualTo(new[] { 6, 7, 2, 1 }));
        }

        /// Tests cases are different from git because of a bit different KDBush implementation
        [TestCase(1, ExpectedResult = 1)]
        [TestCase(33, ExpectedResult = 1)]
        [TestCase(118, ExpectedResult = 21)]
        [TestCase(2178, ExpectedResult = 3)]
        [TestCase(2179, ExpectedResult = 4)]
        public int ReturnsClusterExpansionZoom(int clusterId)
        {
            var cluster = new SuperCluster(DefaultOptions);
            cluster.Load(geoPoints);
            return cluster.GetClusterExpansionZoom(clusterId);
        }

        [Test]
        public void ReturnsClustersWhenQueryCrossesInternationalDateline()
        {
            var data = JsonConvert.DeserializeObject<Feature[]>(@"[
                {
                    type: 'Feature',
                    properties: null,
                    geometry: {
                        type: 'Point',
                        coordinates: [-178.989, 0]
                    }
                }, {
                    type: 'Feature',
                    properties: null,
                    geometry: {
                        type: 'Point',
                        coordinates: [-178.990, 0]
                    }
                }, {
                    type: 'Feature',
                    properties: null,
                    geometry: {
                        type: 'Point',
                        coordinates: [-178.991, 0]
                    }
                }, {
                    type: 'Feature',
                    properties: null,
                    geometry: {
                        type: 'Point',
                        coordinates: [-178.992, 0]
                    }
                }
            ]");

            var cluster = new SuperCluster(DefaultOptions);

            cluster.Load(data);

            var nonCrossing = cluster.GetClusters(new GeoBounds(-10, -179, 10, -177), 1);
            var crossing = cluster.GetClusters(new GeoBounds(-10, 179, 10, -177), 1);

            Assert.That(nonCrossing.Count, Is.GreaterThan(0));
            Assert.That(crossing.Count, Is.GreaterThan(0));

            Assert.That(crossing.Count, Is.EqualTo(nonCrossing.Count));
        }

        [TestCase(129.426390, -103.720017, -445.930843, 114.518236, ExpectedResult = 26)]
        [TestCase(112.207836, -84.578666, -463.149397, 120.169159, ExpectedResult = 27)]
        [TestCase(129.886277, -82.332680, -445.470956, 120.390930, ExpectedResult = 26)]
        [TestCase(458.220043, -84.239039, -117.137190, 120.206585, ExpectedResult = 25)]
        [TestCase(456.713058, -80.354196, -118.644175, 120.539148, ExpectedResult = 25)]
        [TestCase(453.105328, -75.857422, -122.251904, 120.732760, ExpectedResult = 25)]
        public int DoesNotCrashOnWeirdValues(double x, double y, double x1, double y1)
        {
            var cluster = new SuperCluster(DefaultOptions);
            cluster.Load(this.geoPoints);

            return cluster.GetClusters(new GeoBounds(y, x, y1, x1), 1).Count;
        }
    }
}
