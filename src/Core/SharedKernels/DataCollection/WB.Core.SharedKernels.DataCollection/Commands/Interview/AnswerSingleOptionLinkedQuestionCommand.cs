using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootMethod(typeof(Implementation.Aggregates.Interview), "AnswerSingleOptionLinkedQuestion")]
    public class AnswerSingleOptionLinkedQuestionCommand : AnswerQuestionCommand
    {
        public decimal[] SelectedPropagationVector { get; private set; }

        public AnswerSingleOptionLinkedQuestionCommand(Guid interviewId, Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, decimal[] selectedPropagationVector)
            : base(interviewId, userId, questionId, rosterVector, answerTime)
        {
            this.SelectedPropagationVector = selectedPropagationVector;
        }
    }
}