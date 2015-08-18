using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class SingleOptionLinkedQuestionAnswered : QuestionAnswered
    {
        public decimal[] SelectedPropagationVector { get; private set; }

        public SingleOptionLinkedQuestionAnswered(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, decimal[] selectedPropagationVector)
            : base(userId, questionId, rosterVector, answerTime)
        {
            this.SelectedPropagationVector = selectedPropagationVector;
        }
    }
}