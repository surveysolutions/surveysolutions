using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootMethod(typeof(Implementation.Aggregates.Interview), "AnswerTextListQuestion")]
    public class AnswerTextListQuestionCommand : AnswerQuestionCommand
    {
        public Tuple<decimal, string>[] Answers { get; private set; }

        public AnswerTextListQuestionCommand(Guid interviewId, Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, Tuple<decimal, string>[] answers)
            : base(interviewId, userId, questionId, rosterVector, answerTime)
        {
            this.Answers = answers;
        }
    }
}