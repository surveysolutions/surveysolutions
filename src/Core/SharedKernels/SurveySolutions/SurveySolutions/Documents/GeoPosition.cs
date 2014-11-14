using System;

namespace Main.Core.Entities.SubEntities
{
    public class GeoPosition
    {
        public DateTimeOffset Timestamp { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Accuracy { get; set; }

        public double Altitude { get; set; }

        public override string ToString()
        {
            return string.Format("{0},{1}[{2}]{3}", Latitude, Longitude, Accuracy, Altitude);
        }

        public GeoPosition(){}

        public GeoPosition(double latitude, double longitude, double accuracy, double altitude, DateTimeOffset timestamp)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Accuracy = accuracy;
            this.Timestamp = timestamp;
            this.Altitude = altitude;
        }
        
        public static GeoPosition Parse(string value)
        {
            //supports string with and without altitude

            if (string.IsNullOrEmpty(value))
                return null;

            
            var properties = value.Split('[', ']');
            if (properties.Length != 3)
                return null;
            
            var coordinates = properties[0].Split(',');
            if (coordinates.Length != 2)
                return null;

            double latitude;
            if(!double.TryParse(coordinates[0], out latitude))
                return null;

            double longitude;
            if (!double.TryParse(coordinates[1], out longitude))
                return null;

            double accuracy;
            if (!double.TryParse(properties[1], out accuracy))
                return null;


            double altitude;
            if (!String.IsNullOrEmpty(properties[2]))
            {
                if (!double.TryParse(properties[2], out altitude))
                    return null;
            }
            else
            {
                altitude = 0;
            }

            
            return new GeoPosition(latitude, longitude, accuracy, altitude, new DateTimeOffset(DateTime.Now));
        }
    }
}
