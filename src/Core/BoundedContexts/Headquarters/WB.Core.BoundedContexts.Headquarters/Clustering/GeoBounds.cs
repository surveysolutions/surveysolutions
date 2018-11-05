namespace WB.Core.BoundedContexts.Headquarters.Clustering
{
    public class GeoBounds
    {
        /// <summary>
        /// Create new Geo Boundary
        /// </summary>
        /// <param name="south"><c>South</c> - Latitude of South-West point</param>
        /// <param name="west"><c>West</c>  - Longitude of South-West point</param>
        /// <param name="north"><c>North</c> - Latitude of North-East point</param>
        /// <param name="east"><c>East</c>  - Longitude of North-East point</param>
        public GeoBounds(double south, double west, double north, double east)
        {
            West = west;
            South = south;
            East = east;
            North = north;
        }

        public double South { get; private set; }
        public double North { get; private set; }
        public double East { get; private set; }
        public double West { get; private set; }

        /// <summary>
        /// Open bounds, i.e. represent whole world
        /// </summary>
        public static GeoBounds Open => new GeoBounds(-90, -180, 90, 180);

        /// <summary>
        /// Inverse bounds, used to find bounds by geo points
        /// </summary>
        public static GeoBounds Inverse => new GeoBounds(90, 180, -90, -180);

        public void AdjustMinMax(double lat, double lng)
        {
            if (lat < South) South = lat;
            if (lat > North) North = lat;
            if (lng > East)  East  = lng;
            if (lng < West)  West  = lng;
        }
    }
}
