using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class AnswerMultipleOptionsLinkedQuestionCommand: AnswerQuestionCommand
    {
        public decimal[][] SelectedPropagationVectors { get; private set; }

        public AnswerMultipleOptionsLinkedQuestionCommand(Guid interviewId, Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, decimal[][] selectedPropagationVectors)
            : base(interviewId, userId, questionId, rosterVector, answerTime)
        {
            this.SelectedPropagationVectors = selectedPropagationVectors;
        }
    }
}
