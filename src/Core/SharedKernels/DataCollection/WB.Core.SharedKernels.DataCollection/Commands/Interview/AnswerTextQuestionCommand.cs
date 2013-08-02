using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootMethod(typeof(Implementation.Aggregates.Interview), "AnswerTextQuestion")]
    public class AnswerTextQuestionCommand : AnswerQuestionCommand
    {
        public string Answer { get; private set; }

        public AnswerTextQuestionCommand(Guid interviewId, Guid questionId, DateTime answerTime, string answer)
            : base(interviewId, questionId, answerTime)
        {
            this.Answer = answer;
        }
    }
}