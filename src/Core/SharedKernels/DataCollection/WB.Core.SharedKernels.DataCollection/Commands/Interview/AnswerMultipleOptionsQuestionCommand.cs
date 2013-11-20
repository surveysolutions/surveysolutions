using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootMethod(typeof(Implementation.Aggregates.Interview), "AnswerMultipleOptionsQuestion")]
    public class AnswerMultipleOptionsQuestionCommand : AnswerQuestionCommand
    {
        public decimal[] SelectedValues { get; private set; }

        public AnswerMultipleOptionsQuestionCommand(Guid interviewId, Guid userId, Guid questionId, int[] rosterVector, DateTime answerTime, decimal[] selectedValues)
            : base(interviewId, userId, questionId, rosterVector, answerTime)
        {
            this.SelectedValues = selectedValues;
        }
    }
}