using System;

namespace WB.Core.BoundedContexts.Headquarters.Clustering
{
    public class GeoBounds
    {
        /// <summary>
        /// Create new Geo Boundary
        /// </summary>
        /// <param name="south"><c>South</c> - Latitude of South-West point. Minimal latitude</param>
        /// <param name="west"><c>West</c>  - Longitude of South-West point. Minimal longitude</param>
        /// <param name="north"><c>North</c> - Latitude of North-East point. Maximal latitude</param>
        /// <param name="east"><c>East</c>  - Longitude of North-East point. Maximal longitude</param>
        public GeoBounds(double south, double west, double north, double east)
        {
            Longitudes.West = west;
            Latitudes.South = south;
            Longitudes.East = east;
            Latitudes.North = north;
        }

        public double South => Latitudes.South;
        public double North => Latitudes.North;
        public double East => Longitudes.East;
        public double West => Longitudes.West;

        private LongitudePair Longitudes { get; set; } = new LongitudePair();
        private LatitudePair Latitudes { get; set; } = new LatitudePair();

        /// <summary>
        /// Open bounds, i.e. represent whole world
        /// </summary>
        public static GeoBounds Open => new GeoBounds(-90, -180, 90, 180);
        public static GeoBounds Closed => new GeoBounds(1, 180, -1, -180);

        public GeoBounds Expand(double latitude, double longitude)
        {
            this.Latitudes.Extend(latitude);
            this.Longitudes.Extend(longitude);
            return this;
        }

        public int ApproximateGoogleMapsZoomLevel(int clientMapWidth)
        {
            // https://stackoverflow.com/a/6055653/41483
            const int GLOBE_WIDTH = 256; // a constant in Google's map projection

            var angle = East - West;
            if (angle < 0)
            {
                angle += 360;
            }
            
            return (int)Math.Round(Math.Log(clientMapWidth * 360 / angle / GLOBE_WIDTH) / Math.Log(2)) - 1;
        }
        
        private class LongitudePair
        {
            public double West { get; set; } = 180;
            public double East { get; set; } = -180;

            private bool IsEmpty => 360 == West - East;

            public void Extend(double longitude)
            {
                if (!this.Contains(longitude))
                {
                    if (this.IsEmpty)
                    {
                        this.West = this.East = longitude;
                    }
                    else
                    {
                        if (Distance(longitude, this.West) < Distance(this.East, longitude))
                        {
                            this.West = longitude;
                        }
                        else
                        {
                            this.East = longitude;
                        }
                    }
                }
            }

            private bool Contains(double longitude)
            {
                if (-180 == longitude)
                {
                    /** @type {number} */
                    longitude = 180;
                }

                if (West > East)
                {
                    return !this.IsEmpty && (longitude >= West || longitude <= East);
                }
                else
                {
                    return longitude >= West && longitude <= East;
                }
            }

            private double Distance(double a, double b)
            {
                var c = b - a;
                return 0 <= c ? c : b + 180 - (a - 180);
            }
        }

        private class LatitudePair
        {
            public double North { get; set; } = -1;
            public double South { get; set; } = 1;

            private bool IsEmpty => South > North;

            public void Extend(double latitude)
            {
                if (IsEmpty)
                {
                    this.North = this.South = latitude;
                }
                else
                {
                    if (latitude < this.South)
                    {
                        this.South = latitude;
                    }
                    else
                    {
                        if (latitude > this.North)
                        {
                            this.North = latitude;
                        }
                    }
                }
            }

            public bool Contains(double latitude) => latitude >= South && latitude <= North;
        }
    }
}
