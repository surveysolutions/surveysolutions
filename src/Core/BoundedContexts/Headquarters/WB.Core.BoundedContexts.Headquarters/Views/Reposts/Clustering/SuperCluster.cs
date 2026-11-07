// Port of https://github.com/mapbox/supercluster/blob/v4.1.1/index.js
//
// ISC License
//
// Copyright (c) 2016, Mapbox
//
// Permission to use, copy, modify, and/or distribute this software for any purpose
// with or without fee is hereby granted, provided that the above copyright notice
// and this permission notice appear in all copies.
//
// THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH
// REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT,
// INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS
// OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER
// TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF
// THIS SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Clustering
{
    public class Cluster
    {
        public int Zoom { get; set; }
        public int Index { get; set; }
        public int ParentId { get; set; }
        public int NumPoints { get; set; } = 1;
        public IDictionary<string, object> Props { get; set; }
    }

    public class SuperCluster
    {
        private const int MinZoom = 3;
        private const int MaxZoom = 16;
        private const int Radius = 90;
        private const int Extent = 512;
        private const int NodeSize = 64;

        private readonly KdTree[] trees = new KdTree[MaxZoom + 2];

        public void Load(IEnumerable<Feature> features)
        {
            // generate a cluster object for each point and index input points into a KD-tree
            var clusters = features
                .Where(f => f.Geometry is Point)
                .Select((f, i) => CreatePointCluster(f, i))
                .ToList();

            var leaf = new KdTree(NodeSize);
            leaf.Index(clusters);
            trees[MaxZoom + 1] = leaf;

            // cluster points on max zoom, then cluster the results on previous zoom, etc.;
            // results in a cluster hierarchy across zoom levels
            for (var z = MaxZoom; z >= MinZoom; z--)
            {
                clusters = Cluster(clusters, z);
                var tree = new KdTree(NodeSize);
                tree.Index(clusters);
                trees[z] = tree;
            }
        }

        public List<ClusterPoint> GetClusters(GeoBounds bounds, int zoom)
        {
            var minLng = ((bounds.West + 180) % 360 + 360) % 360 - 180;
            var minLat = Math.Max(-90, Math.Min(90, bounds.South));
            var maxLng = Math.Abs(bounds.East - 180) < 0.01 ? 180 : ((bounds.East + 180) % 360 + 360) % 360 - 180;
            var maxLat = Math.Max(-90, Math.Min(90, bounds.North));

            if (bounds.East - bounds.West >= 360)
            {
                minLng = -180;
                maxLng = 180;
            }
            else if (minLng > maxLng)
            {
                var easternHem = GetClusters(new GeoBounds(minLat, minLng, maxLat, 180), zoom);
                var westernHem = GetClusters(new GeoBounds(minLat, -180, maxLat, maxLng), zoom);
                easternHem.AddRange(westernHem);
                return easternHem;
            }

            var tree = trees[LimitZoom(zoom)];
            return tree.Query(LngX(minLng), LatY(minLat), LngX(maxLng), LatY(maxLat));
        }

        public int GetClusterExpansionZoom(int clusterId)
        {
            var clusterZoom = clusterId % 32 - 1;

            while (clusterZoom <= MaxZoom)
            {
                var children = GetChildren(clusterId);
                clusterZoom++;
                if (children.Count != 1) break;
                clusterId = children[0].UserData.Index;
            }

            return clusterZoom;
        }

        private List<ClusterPoint> GetChildren(int clusterId)
        {
            var originId = clusterId >> 5;
            var originZoom = clusterId % 32;
            const string errorMsg = "No cluster with the specified id.";

            var index = trees[originZoom] ?? throw new InvalidOperationException(errorMsg);
            var origin = index.GetByOriginIndex(originId) ?? throw new InvalidOperationException(errorMsg);

            var r = Radius / (Extent * Math.Pow(2, originZoom - 1));
            return index.Query(origin.X, origin.Y, r)
                .Where(child => child.UserData.ParentId == clusterId)
                .ToList();
        }

        private static int LimitZoom(int zoom) => Math.Max(MinZoom, Math.Min(zoom, MaxZoom + 1));

        private List<ClusterPoint> Cluster(List<ClusterPoint> points, int zoom)
        {
            var clusters = new List<ClusterPoint>();
            var r = Radius / (Extent * Math.Pow(2, zoom));

            for (var i = 0; i < points.Count; i++)
            {
                var p = points[i];

                // if we've already visited the point at this zoom level, skip it
                if (p.UserData.Zoom <= zoom) continue;

                p.UserData.Zoom = zoom;

                var neighbors = trees[zoom + 1].Query(p.X, p.Y, r);

                var numPoints = p.UserData.NumPoints;
                var wx = p.X * numPoints;
                var wy = p.Y * numPoints;

                // encode both zoom and point index on which the cluster originated
                var id = (i << 5) + (zoom + 1);

                foreach (var neighbor in neighbors)
                {
                    // filter out neighbors that are already processed
                    if (neighbor.UserData.Zoom <= zoom) continue;

                    neighbor.UserData.Zoom = zoom;

                    var innerPoints = neighbor.UserData.NumPoints;
                    wx += neighbor.X * innerPoints;
                    wy += neighbor.Y * innerPoints;

                    numPoints += innerPoints;
                    neighbor.UserData.ParentId = id;
                }

                if (numPoints == 1)
                {
                    clusters.Add(p);
                }
                else
                {
                    p.UserData.ParentId = id;
                    clusters.Add(new ClusterPoint(wx / numPoints, wy / numPoints, new Cluster
                    {
                        Index = id,
                        ParentId = -1,
                        NumPoints = numPoints,
                        Zoom = int.MaxValue
                    }));
                }
            }

            return clusters;
        }

        private static ClusterPoint CreatePointCluster(Feature feature, int id)
        {
            var point = (Point)feature.Geometry;
            return new ClusterPoint(LngX(point.Coordinates.Longitude), LatY(point.Coordinates.Latitude),
                new Cluster
                {
                    Zoom = int.MaxValue,
                    Index = id,
                    ParentId = -1,
                    Props = feature.Properties
                });
        }

        // longitude/latitude to spherical mercator in [0..1] range
        private static double LngX(double lng) => lng / 360 + 0.5;

        private static double LatY(double lat)
        {
            var sin = Math.Sin(lat * Math.PI / 180);
            var y = 0.5 - 0.25 * Math.Log((1 + sin) / (1 - sin)) / Math.PI;
            return y < 0 ? 0 : y > 1 ? 1 : y;
        }
    }
}
