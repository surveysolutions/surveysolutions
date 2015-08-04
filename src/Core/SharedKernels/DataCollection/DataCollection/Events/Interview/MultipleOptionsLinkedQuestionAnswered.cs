using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class MultipleOptionsLinkedQuestionAnswered : QuestionAnswered
    {
        public decimal[][] SelectedPropagationVectors { get; private set; }

        public MultipleOptionsLinkedQuestionAnswered(Guid userId, Guid questionId, decimal[] propagationVector, DateTime answerTime, decimal[][] selectedPropagationVectors)
            : base(userId, questionId, propagationVector, answerTime)
        {
            this.SelectedPropagationVectors = selectedPropagationVectors;
        }
    }
}
