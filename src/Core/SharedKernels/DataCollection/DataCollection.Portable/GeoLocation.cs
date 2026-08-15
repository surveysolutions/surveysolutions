using System;

namespace WB.Core.SharedKernels.DataCollection
{
    public class GeoLocation
    {
        public GeoLocation(double latitude, double longitude, double? accuracy, double? altitude, DateTimeOffset timestamp = default)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Accuracy = accuracy;
            this.Altitude = altitude;
            this.Timestamp = timestamp;
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Accuracy { get; set; }
        public double? Altitude { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}
