using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootMethod(typeof(Implementation.Aggregates.Interview), "AnswerMultipleOptionsQuestion")]
    public class AnswerMultipleOptionsQuestionCommand : AnswerQuestionCommand
    {
        public Guid[] SelectedOptions { get; private set; }

        public AnswerMultipleOptionsQuestionCommand(Guid interviewId, Guid userId, Guid questionId, DateTime answerTime, Guid[] selectedOptions)
            : base(interviewId, userId, questionId, answerTime)
        {
            this.SelectedOptions = selectedOptions;
        }
    }
}