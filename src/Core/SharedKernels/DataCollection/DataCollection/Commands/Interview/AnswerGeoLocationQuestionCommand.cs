using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class AnswerGeoLocationQuestionCommand : AnswerQuestionCommand
    {
        public DateTimeOffset Timestamp { get; private set; }
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public double Accuracy { get; private set; }
        public double Altitude { get; private set; }

        public AnswerGeoLocationQuestionCommand(Guid interviewId, Guid userId, Guid questionId, decimal[] rosterVector,
            double latitude, double longitude, double accuracy, double altitude, 
            DateTimeOffset timestamp)
            : base(interviewId, userId, questionId, rosterVector)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Accuracy = accuracy;
            this.Timestamp = timestamp;
            this.Altitude = altitude;
        }
    }
}
