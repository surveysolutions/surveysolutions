using System;
using System.Collections.Generic;
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

        public void Load(IEnumerable<GeoPoint> points)
        {
            // generate a cluster object for each point and index input points into a KD-tree
            var clusters = new List<Point<Cluster>>();
            int i = 0;
            foreach (var point in points)
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

        public List<Point<Cluster>> GetClusters(int zoom, params double[] bbox)
        {
            return GetClusters(bbox, zoom);
        }

        public List<Point<Cluster>> GetClusters(double[] bbox, int zoom)
        {
            var minLng = ((bbox[0] + 180) % 360 + 360) % 360 - 180;
            var minLat = Math.Max(-90, Math.Min(90, bbox[1]));
            var maxLng = Math.Abs(bbox[2] - 180) < 0.01 ? 180 : ((bbox[2] + 180) % 360 + 360) % 360 - 180;
            var maxLat = Math.Max(-90, Math.Min(90, bbox[3]));

            if (bbox[2] - bbox[0] >= 360)
            {
                minLng = -180;
                maxLng = 180;
            }
            else if (minLng > maxLng)
            {
                var easternHem = GetClusters(new[] { minLng, minLat, 180, maxLat }, zoom);
                var westernHem = GetClusters(new[] { -180, minLat, maxLng, maxLat }, zoom);
                easternHem.AddRange(westernHem);
                return easternHem;
            }

            var tree = this.trees[this.limitZoom(zoom)];
            return tree.Query(lngX(minLng), latY(minLat), lngX(maxLng), latY(maxLat));
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

                var numPoints = p.UserData.NumPoints ?? 1;
                var wx = p.X * numPoints;
                var wy = p.Y * numPoints;

                /*
                     if (reduce) {
                        clusterProperties = initial();
                        this._accumulate(clusterProperties, p);
                    }       
                */

                // encode both zoom and point index on which the cluster originated
                var id = (i << 5) + (zoom + 1);

                foreach (var neighbor in neighbors)
                {
                    // filter out neighbors that are already processed
                    if (neighbor.UserData.Zoom <= zoom) continue;

                    neighbor.UserData.Zoom = zoom;// save the zoom (so it doesn't get processed twice)

                    var innerPoints = neighbor.UserData.NumPoints ?? 1;
                    wx += neighbor.X * innerPoints;
                    wy += neighbor.Y * innerPoints;

                    numPoints += innerPoints;

                    /*
                        if (reduce) {
                            this._accumulate(clusterProperties, b);
                        }    
                    */
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

        Point<Cluster> CreatePointCluster(GeoPoint p, int id)
        {
            return new Point<Cluster>(lngX(p.Position[0]), latY(p.Position[1]),
                new Cluster
                {
                    Zoom = int.MaxValue,
                    Index = id,
                    ParentId = -1,
                    Props = p.Props
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

        public class GeoPoint
        {
            public double[] Position { get; set; }
            public Dictionary<string, object> Props { get; set; }
        }

        public class SuperClusterOptions
        {
            /// <summary>
            /// Minimum zoom level at which clusters are generated.
            /// </summary>
            public int MinZoom { get; set; } = 0;

            /// <summary>
            /// Maximum zoom level at which clusters are generated.
            /// </summary>
            public int MaxZoom { get; set; } = 16;

            /// <summary>
            /// Cluster radius, in pixels.
            /// </summary>
            public int Radius { get; set; } = 60;

            /// <summary>
            /// (Tiles) Tile extent. Radius is calculated relative to this value
            /// </summary>
            public int Extent { get; set; } = 256;

            /// <summary>
            /// Size of the KD-tree leaf node. Affects performance.
            /// </summary>
            public int NodeSize { get; set; } = 64;
        }
    }
}
