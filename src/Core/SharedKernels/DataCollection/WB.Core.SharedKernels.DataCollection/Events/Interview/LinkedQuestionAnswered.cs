using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class LinkedQuestionAnswered : QuestionAnswered
    {
        public int[] SelectedPropagationVector { get; private set; }

        public LinkedQuestionAnswered(Guid userId, Guid questionId, int[] propagationVector, DateTime answerTime, int[] selectedPropagationVector)
            : base(userId, questionId, propagationVector, answerTime)
        {
            this.SelectedPropagationVector = selectedPropagationVector;
        }
    }
}