using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class MultipleOptionsQuestionAnswered : QuestionAnswered
    {
        public decimal[] SelectedValues { get; private set; }

        public MultipleOptionsQuestionAnswered(Guid userId, Guid questionId, decimal[] rosterVector, 
            DateTimeOffset originDate, decimal[] selectedValues, DateTime? answerTimeUtc = null)
            : base(userId, questionId, rosterVector, originDate, answerTimeUtc)
        {
            this.SelectedValues = selectedValues;
        }
    }
}
