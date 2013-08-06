using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootMethod(typeof(Implementation.Aggregates.Interview), "AnswerSingleOptionQuestion")]
    public class AnswerSingleOptionQuestionCommand : AnswerQuestionCommand
    {
        public decimal SelectedValue { get; private set; }

        public AnswerSingleOptionQuestionCommand(Guid interviewId, Guid userId, Guid questionId, DateTime answerTime, decimal selectedValue)
            : base(interviewId, userId, questionId, answerTime)
        {
            this.SelectedValue = selectedValue;
        }
    }
}