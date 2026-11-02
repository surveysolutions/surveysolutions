#nullable enable
using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class GeoLocationQuestionAnswered : QuestionAnswered
    {
        public DateTimeOffset Timestamp { get; private set; }
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public double? Accuracy { get; private set; }
        public double? Altitude { get; private set; }

        /// <summary>Name of the location provider that produced this fix (e.g. "gps", "network", "fused").</summary>
        public string? GpsProvider { get; private set; }

        /// <summary>True when the fix was produced by a mock provider (external GPS sensors are exposed this way on Android).</summary>
        public bool IsFromMockProvider { get; private set; }

        public GeoLocationQuestionAnswered(Guid userId, Guid questionId, decimal[] rosterVector, DateTimeOffset originDate,
            double latitude, double longitude, double? accuracy, double? altitude, DateTimeOffset timestamp,
            string? gpsProvider = null, bool isFromMockProvider = false) 
            : base(userId, questionId, rosterVector, originDate)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Accuracy = accuracy;
            this.Timestamp = timestamp;
            this.Altitude = altitude;
            this.GpsProvider = gpsProvider;
            this.IsFromMockProvider = isFromMockProvider;
        }
    }
}
