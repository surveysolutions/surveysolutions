using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootMethod(typeof(Implementation.Aggregates.Interview), "AnswerNumericQuestion")]
    public class AnswerNumericQuestionCommand : AnswerQuestionCommand
    {
        public decimal Answer { get; private set; }

        public AnswerNumericQuestionCommand(Guid interviewId, Guid userId, Guid questionId, DateTime answerTime, decimal answer)
            : base(interviewId, userId, questionId, answerTime)
        {
            this.Answer = answer;
        }
    }
}