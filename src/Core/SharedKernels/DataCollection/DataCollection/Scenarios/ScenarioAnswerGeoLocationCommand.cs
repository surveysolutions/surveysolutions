using System;

namespace WB.Core.SharedKernels.DataCollection.Scenarios
{
    public class ScenarioAnswerGeoLocationCommand : ScenarioAnswerCommand
    {
        public ScenarioAnswerGeoLocationCommand(string variable, RosterVector rosterVector, DateTimeOffset timestamp, double latitude, double longitude, double accuracy, double altitude) : base(variable, rosterVector)
        {
            Timestamp = timestamp;
            Latitude = latitude;
            Longitude = longitude;
            Accuracy = accuracy;
            Altitude = altitude;
        }

        public DateTimeOffset Timestamp { get; }
        public double Latitude { get; }
        public double Longitude { get; }
        public double Accuracy { get; }
        public double Altitude { get; }
    }
}
