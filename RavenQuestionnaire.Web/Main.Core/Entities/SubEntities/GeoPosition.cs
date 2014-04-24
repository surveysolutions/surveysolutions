using System;

namespace Main.Core.Entities.SubEntities
{
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

        public GeoPosition(double latitude, double longitude, double accuracy, DateTimeOffset timestamp)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Accuracy = accuracy;
            this.Timestamp = timestamp;
        }


        public GeoPosition(string position)
        {
            var coordinates = position.Split(',');
            Latitude = Double.Parse(coordinates[0]);
            Longitude = Double.Parse(coordinates[1]);
        }

        public static GeoPosition Parse(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            var subStringWithCoordinates = value.Substring(0, value.IndexOf('['));
            if(string.IsNullOrEmpty(subStringWithCoordinates))
                return null;
            var coordinates = subStringWithCoordinates.Split(',');
            if(coordinates.Length!=2)
                return null;

            double latitude;
            double longitude;
            if(!double.TryParse(coordinates[0], out latitude))
                return null;

            if (!double.TryParse(coordinates[1], out longitude))
                return null;

            var accuracyString = value.Substring(subStringWithCoordinates.Length + 1, value.Length - subStringWithCoordinates.Length - 2);
            double accuracy;
            if (!double.TryParse(accuracyString, out accuracy))
                return null;
            return new GeoPosition(latitude, longitude, accuracy, new DateTimeOffset(DateTime.Now));
        }
    }
}
