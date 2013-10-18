using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootMethod(typeof(Implementation.Aggregates.Interview), "AnswerTextQuestion")]
    public class AnswerTextQuestionCommand : AnswerQuestionCommand
    {
        public string Answer { get; private set; }

        public AnswerTextQuestionCommand(Guid interviewId, Guid userId, Guid questionId, int[] propagationVector, DateTime answerTime, string answer)
            : base(interviewId, userId, questionId, propagationVector, answerTime)
        {
            this.Answer = answer;
        }
    }
}