using System;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    internal class GeoLocationPoint
    {
        public GeoLocationPoint(double latitude, double longitude, double accuracy, double altitude, DateTimeOffset timestamp)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Accuracy = accuracy;
            this.Timestamp = timestamp;
            this.Altitude = altitude;
        }

        public DateTimeOffset Timestamp { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Accuracy { get; set; }

        public double Altitude { get; set; }

    }
}
