using System;
using System.Collections.Generic;
using System.Linq;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using KDBush;

namespace WB.Core.BoundedContexts.Headquarters.Clustering
{
    /// <summary>
    /// Port from javascript library from MapBox for clustering
    /// https://github.com/mapbox/supercluster/blob/v4.1.1/index.js Copyright (c) 2016, Mapbox
    /// </summary>
    public class SuperCluster
    {
        private readonly SuperClusterOptions _options;
        private readonly KDBush<Cluster>[] trees;

        public SuperCluster(SuperClusterOptions options = null)
        {
            _options = options ?? new SuperClusterOptions();
            this.trees = new KDBush<Cluster>[_options.MaxZoom + 2];
        }

        public void Load(IEnumerable<Feature> features)
        {
            // generate a cluster object for each point and index input points into a KD-tree
            var clusters = new List<Point<Cluster>>();
            int i = 0;

            foreach (var point in features.Where(f => f.Geometry is Point))
            {
                var cluster = CreatePointCluster(point, i++);
                clusters.Add(cluster);
            }

            var leaf = new KDBush<Cluster>(_options.NodeSize);
            leaf.Index(clusters);

            this.trees[_options.MaxZoom + 1] = leaf;

            // cluster points on max zoom, then cluster the results on previous zoom, etc.;
            // results in a cluster hierarchy across zoom levels
            for (var z = _options.MaxZoom; z >= _options.MinZoom; z--)
            {
                clusters = Cluster(clusters, z);
                var leafz = new KDBush<Cluster>(_options.NodeSize);
                leafz.Index(clusters);
                this.trees[z] = leafz;
            }
        }
        
        public List<Point<Cluster>> GetClusters(GeoBounds bounds, int zoom)
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

            var tree = this.trees[this.limitZoom(zoom)];
            return tree.Query(lngX(minLng), latY(minLat), lngX(maxLng), latY(maxLat));
        }
        
        public int GetClusterExpansionZoom(int clusterId)
        {
            int clusterZoom = clusterId % 32 - 1;

            while (clusterZoom <= this._options.MaxZoom)
            {
                var children = this.GetChildren(clusterId);
                clusterZoom++;
                if (children.Count != 1) break;
                clusterId = children[0].UserData.Index;
            }

            return clusterZoom;
        }
        
        public List<Point<Cluster>> GetChildren(int clusterId)
        {
            var originId = clusterId >> 5;
            var originZoom = clusterId % 32;
            const string errorMsg = "No cluster with the specified id.";

            var index = this.trees[originZoom];
            if (index == null) throw new Exception(errorMsg);

            var origin = index.GetByOriginIndex(originId);
            if (origin == null) throw new Exception(errorMsg);

            var r = this._options.Radius / (this._options.Extent * Math.Pow(2, originZoom - 1));
            return index.Query(origin.X, origin.Y, r);
        }

        int limitZoom(int zoom)
        {
            return Math.Max(this._options.MinZoom, Math.Min(zoom, this._options.MaxZoom + 1));
        }

        List<Point<Cluster>> Cluster(List<Point<Cluster>> points, int zoom)
        {
            var clusters = new List<Point<Cluster>>();
            var r = _options.Radius / (_options.Extent * Math.Pow(2, zoom));

            for (int i = 0; i < points.Count; i++)
            {
                Point<Cluster> p = points[i];

                // if we've already visited the point at this zoom level, skip it
                if (p.UserData.Zoom <= zoom) continue;

                p.UserData.Zoom = zoom;

                var tree = this.trees[zoom + 1];
                var neighbors = tree.Query(p.X, p.Y, r);

                var numPoints = p.UserData.NumPoints;
                var wx = p.X * numPoints;
                var wy = p.Y * numPoints;
                
                // encode both zoom and point index on which the cluster originated
                var id = (i << 5) + (zoom + 1);

                foreach (var neighbor in neighbors)
                {
                    // filter out neighbors that are already processed
                    if (neighbor.UserData.Zoom <= zoom) continue;

                    neighbor.UserData.Zoom = zoom;// save the zoom (so it doesn't get processed twice)

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
                    clusters.Add(new Point<Cluster>(wx / numPoints, wy / numPoints, new Cluster
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

        Point<Cluster> CreatePointCluster(Feature p, int id)
        {
            var point = p.Geometry as Point;
            return new Point<Cluster>(lngX(point.Coordinates.Longitude), latY(point.Coordinates.Latitude),
                new Cluster
                {
                    Zoom = int.MaxValue,
                    Index = id,
                    ParentId = -1,
                    Props = p.Properties
                });
        }

        // longitude/latitude to spherical mercator in [0..1] range
        double lngX(double lng)
        {
            return lng / 360 + 0.5;
        }
        double latY(double lat)
        {
            double sin = Math.Sin(lat * Math.PI / 180);
            var y = (0.5 - 0.25 * Math.Log((1 + sin) / (1 - sin)) / Math.PI);
            return y < 0 ? 0 : y > 1 ? 1 : y;
        }

        public class SuperClusterOptions
        {
            /// <summary>
            /// Minimum zoom level at which clusters are generated.
            /// </summary>
            public int MinZoom { get; set; } = 3;

            /// <summary>
            /// Maximum zoom level at which clusters are generated.
            /// </summary>
            public int MaxZoom { get; set; } = 16;

            /// <summary>
            /// Cluster radius, in pixels.
            /// </summary>
            public int Radius { get; set; } = 90;

            /// <summary>
            /// (Tiles) Tile extent. Radius is calculated relative to this value
            /// </summary>
            public int Extent { get; set; } = 512;

            /// <summary>
            /// Size of the KD-tree leaf node. Affects performance.
            /// </summary>
            public int NodeSize { get; set; } = 64;
        }
    }
}
