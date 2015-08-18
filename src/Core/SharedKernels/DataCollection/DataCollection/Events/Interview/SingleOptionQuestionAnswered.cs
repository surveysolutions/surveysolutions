using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class SingleOptionQuestionAnswered : QuestionAnswered
    {
        public decimal SelectedValue { get; private set; }

        public SingleOptionQuestionAnswered(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTime, decimal selectedValue)
            : base(userId, questionId, rosterVector, answerTime)
        {
            this.SelectedValue = selectedValue;
        }
    }
}