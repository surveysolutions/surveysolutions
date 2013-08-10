using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class SingleOptionQuestionAnswered : QuestionAnswered
    {
        public decimal SelectedValue { get; private set; }

        public SingleOptionQuestionAnswered(Guid userId, Guid questionId, int[] propagationVector, DateTime answerTime, decimal selectedValue)
            : base(userId, questionId, propagationVector, answerTime)
        {
            this.SelectedValue = selectedValue;
        }
    }
}