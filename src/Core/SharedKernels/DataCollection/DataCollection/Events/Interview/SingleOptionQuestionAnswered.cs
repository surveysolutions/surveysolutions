using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class SingleOptionQuestionAnswered : QuestionAnswered
    {
        public decimal SelectedValue { get; private set; }

        public SingleOptionQuestionAnswered(Guid userId, Guid questionId, decimal[] rosterVector, 
            DateTimeOffset originDate, decimal selectedValue)
            : base(userId, questionId, rosterVector, originDate)
        {
            this.SelectedValue = selectedValue;
        }
    }
}
