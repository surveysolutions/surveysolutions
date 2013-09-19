using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class GeoLocationQuestionAnswered : QuestionAnswered
    {
        public DateTimeOffset Timestamp { get; private set; }
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public double Accuracy { get; private set; }

        public GeoLocationQuestionAnswered(Guid userId, Guid questionId, int[] propagationVector, DateTime answerTime,
            double latitude, double longitude, double accuracy, DateTimeOffset timestamp) 
            : base(userId, questionId, propagationVector, answerTime)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Accuracy = accuracy;
            this.Timestamp = timestamp;
        }
    }
}
