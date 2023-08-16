using System;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class GpsLocation
    {
        public GpsLocation(double? accuracy, double? altitude, double latitude, double longitude, DateTimeOffset timestamp)
        {
            this.Accuracy = accuracy;
            this.Altitude = altitude;
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Timestamp = timestamp;
        }

        public double? Accuracy { get; private set; }
        public double? Altitude { get; private set; }
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public DateTimeOffset Timestamp { get; private set; }
    }
}