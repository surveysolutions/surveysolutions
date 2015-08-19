using System;

namespace WB.Core.SharedKernels.Enumerator.Entities.Interview
{
    public class GpsCoordinatesAnswer : BaseInterviewAnswer
    {
        public double? Latitude { get; private set; }
        public double? Longitude { get; private set; }
        public double? Accuracy { get; private set; }
        public double? Altitude { get; private set; }

        public GpsCoordinatesAnswer() { }
        public GpsCoordinatesAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }

        public void SetAnswer(double latitude, double longitude, double accuracy, double altitude)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Accuracy = accuracy;
            this.Altitude = altitude;
        }

        public override bool IsAnswered
        {
            get { return this.Latitude.HasValue && this.Longitude.HasValue && this.Altitude.HasValue && this.Accuracy.HasValue; }
        }

        public override void RemoveAnswer()
        {
            this.Latitude = null;
            this.Longitude = null;
            this.Accuracy = null;
            this.Altitude = null;
        }
    }
}
