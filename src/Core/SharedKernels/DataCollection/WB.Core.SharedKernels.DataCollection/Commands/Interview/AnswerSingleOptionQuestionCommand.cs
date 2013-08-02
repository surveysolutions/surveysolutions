using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootMethod(typeof(Implementation.Aggregates.Interview), "AnswerSingleOptionQuestion")]
    public class AnswerSingleOptionQuestionCommand : AnswerQuestionCommand
    {
        public Guid SelectedOption { get; private set; }

        public AnswerSingleOptionQuestionCommand(Guid interviewId, Guid userId, Guid questionId, DateTime answerTime, Guid selectedOption)
            : base(interviewId, userId, questionId, answerTime)
        {
            this.SelectedOption = selectedOption;
        }
    }
}