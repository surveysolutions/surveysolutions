namespace Main.Core.Entities.SubEntities
{
    using System;

    public class GeoPosition
    {
        public DateTimeOffset Timestamp { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Accuracy { get; set; }

        public override string ToString()
        {
            return string.Format("{0},{1}[{2}]", Latitude, Longitude, Accuracy);
        }

        public GeoPosition()
        {
        }

        public GeoPosition(string position)
        {
            var coordinates = position.Split(',');
            Latitude = Double.Parse(coordinates[0]);
            Longitude = Double.Parse(coordinates[1]);
        }
    }
}
