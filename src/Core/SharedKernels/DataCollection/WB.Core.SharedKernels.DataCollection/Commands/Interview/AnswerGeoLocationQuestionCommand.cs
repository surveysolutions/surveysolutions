using System;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootMethod(typeof(Implementation.Aggregates.Interview), "AnswerGeoLocationQuestion")]
    public class AnswerGeoLocationQuestionCommand : AnswerQuestionCommand
    {
        public GeoPosition Answer { get; private set; }

        public AnswerGeoLocationQuestionCommand(Guid interviewId, Guid userId, Guid questionId, int[] propagationVector, DateTime answerTime, GeoPosition answer)
            : base(interviewId, userId, questionId, propagationVector, answerTime)
        {
            this.Answer = answer;
        }
    }
}