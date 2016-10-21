using System;
using System.Globalization;
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
            return string.Format(CultureInfo.InvariantCulture, "{0},{1}[{2}]{3}", Latitude, Longitude, Accuracy, Altitude);
        }

        public static GeoPosition FromString(string value)
        {
            string[] stringParts = value.Split(',', '[', ']');

            return new GeoPosition(
                double.Parse(stringParts[0], CultureInfo.InvariantCulture),
                double.Parse(stringParts[1], CultureInfo.InvariantCulture),
                double.Parse(stringParts[2], CultureInfo.InvariantCulture),
                double.Parse(stringParts[3], CultureInfo.InvariantCulture),
                default(DateTimeOffset));
        }

        public static object ParseProperty(string value, string propertyName)
        {
            if (!PropertyNames.Any(p => p.Equals(propertyName, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException(
                    String.Format("{0} property is missing at GeoPosition object. Value {1} can't be parsed",
                        propertyName,
                        value));

            if (propertyName.Equals("Timestamp", StringComparison.OrdinalIgnoreCase))
                return DateTimeOffset.Parse(value, CultureInfo.InvariantCulture.DateTimeFormat);

            return double.Parse(value, CultureInfo.InvariantCulture.NumberFormat);
        }

        public static string[] PropertyNames { get { return propertyNames; } }

        public override bool Equals(object obj)
        {
            GeoPosition geoPosition = obj as GeoPosition;
            return geoPosition != null && this.Equals(geoPosition);
        }

        protected bool Equals(GeoPosition other) => this.Latitude.Equals(other.Latitude) && this.Longitude.Equals(other.Longitude) &&
                                                    this.Accuracy.Equals(other.Accuracy) && this.Altitude.Equals(other.Altitude) &&
                                                    this.Timestamp.Equals(other.Timestamp);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.Latitude.GetHashCode();
                hashCode = (hashCode*397) ^ this.Longitude.GetHashCode();
                hashCode = (hashCode*397) ^ this.Accuracy.GetHashCode();
                hashCode = (hashCode*397) ^ this.Altitude.GetHashCode();
                hashCode = (hashCode*397) ^ this.Timestamp.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(GeoPosition x, GeoPosition y) => x?.Accuracy == y?.Accuracy &&
                                                                        x?.Altitude == y?.Altitude &&
                                                                        x?.Latitude == y?.Latitude &&
                                                                        x?.Longitude == y?.Longitude &&
                                                                        x?.Timestamp == y?.Timestamp;

        public static bool operator !=(GeoPosition x, GeoPosition y) => !(x == y);

        public GeoPosition Clone()
        {
            return new GeoPosition(Latitude, Longitude, Accuracy, Altitude, Timestamp);
        }
    }
}
