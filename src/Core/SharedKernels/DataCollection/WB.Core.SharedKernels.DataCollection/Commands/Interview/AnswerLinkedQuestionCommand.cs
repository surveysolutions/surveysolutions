using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootMethod(typeof(Implementation.Aggregates.Interview), "AnswerLinkedQuestion")]
    public class AnswerLinkedQuestionCommand : AnswerQuestionCommand
    {
        public int[] SelectedPropagationVector { get; private set; }

        public AnswerLinkedQuestionCommand(Guid interviewId, Guid userId, Guid questionId, int[] propagationVector, DateTime answerTime, int[] selectedPropagationVector)
            : base(interviewId, userId, questionId, propagationVector, answerTime)
        {
            this.SelectedPropagationVector = selectedPropagationVector;
        }
    }
}