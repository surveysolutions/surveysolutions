using System;
using System.Collections.Generic;
using System.Linq;

namespace Main.Core.Entities.SubEntities
{
    public class GeoPosition
    {
        private static string[] propertyNames = new[] {"Latitude", "Longitude", "Accuracy", "Altitude", "Timestamp"};
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Accuracy { get; set; }
        public double Altitude { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        public GeoPosition(){}

        public GeoPosition(dynamic propertyAnswerMap)
        {
            this.Latitude = propertyAnswerMap.Latitude;

            this.Longitude = propertyAnswerMap.Longitude;

            if (IsPropertyExist(propertyAnswerMap, "Accuracy"))
                this.Accuracy = propertyAnswerMap.Accuracy;

            if (IsPropertyExist(propertyAnswerMap, "Timestamp"))
                this.Timestamp = propertyAnswerMap.Timestamp;

            if (IsPropertyExist(propertyAnswerMap, "Altitude"))
                this.Altitude = propertyAnswerMap.Altitude;
        }

        private bool IsPropertyExist(dynamic propertyAnswerMap, string name)
        {
            return ((IDictionary<String, object>)propertyAnswerMap).ContainsKey(name);
        }

        public GeoPosition(double latitude, double longitude, double accuracy, double altitude, DateTimeOffset timestamp)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Accuracy = accuracy;
            this.Timestamp = timestamp;
            this.Altitude = altitude;
        }

        public override string ToString()
        {
            return string.Format("{0},{1}[{2}]{3}", Latitude, Longitude, Accuracy, Altitude);
        }

        public static object ParseProperty(string value, string propertyName)
        {
            if (!PropertyNames.Contains(propertyName))
                throw new ArgumentException(
                    String.Format("{0} property is missing at GeoPosition object. Value {1} can't be parsed",
                        propertyName,
                        value));

            if (propertyName == "Timestamp")
                return DateTimeOffset.Parse(value);

            return double.Parse(value);
        }

        public static string[] PropertyNames { get { return propertyNames; } }
    }
}
