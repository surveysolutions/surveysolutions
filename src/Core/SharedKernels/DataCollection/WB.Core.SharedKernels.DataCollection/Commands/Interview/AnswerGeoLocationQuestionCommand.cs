using System;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootMethod(typeof(Implementation.Aggregates.Interview), "AnswerGeoLocationQuestion")]
    public class AnswerGeoLocationQuestionCommand : AnswerQuestionCommand
    {
        public DateTimeOffset Timestamp { get; private set; }
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public double Accuracy { get; private set; }

        public AnswerGeoLocationQuestionCommand(Guid interviewId, Guid userId, Guid questionId, decimal[] rosterVector,
            DateTime answerTime, double latitude, double longitude, double accuracy, DateTimeOffset timestamp)
            : base(interviewId, userId, questionId, rosterVector, answerTime)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Accuracy = accuracy;
            this.Timestamp = timestamp;
        }
    }
}