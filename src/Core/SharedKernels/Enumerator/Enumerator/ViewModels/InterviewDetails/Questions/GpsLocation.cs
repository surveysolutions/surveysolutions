using System;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class GpsLocation
    {
        public GpsLocation(double? accuracy, double? altitude, double latitude, double longitude, DateTimeOffset timestamp,
            string provider = null, bool isFromMockProvider = false)
        {
            this.Accuracy = accuracy;
            this.Altitude = altitude;
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Timestamp = timestamp;
            this.Provider = provider;
            this.IsFromMockProvider = isFromMockProvider;
        }

        public double? Accuracy { get; private set; }
        public double? Altitude { get; private set; }
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public DateTimeOffset Timestamp { get; private set; }

        /// <summary>Name of the Android location provider that produced this fix (e.g. "gps", "network", "fused").</summary>
        public string Provider { get; private set; }

        /// <summary>True when the fix was produced by a mock provider (external GPS sensors are exposed this way on Android).</summary>
        public bool IsFromMockProvider { get; private set; }
    }
}