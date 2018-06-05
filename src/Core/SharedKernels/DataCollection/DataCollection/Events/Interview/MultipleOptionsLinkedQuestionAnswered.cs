using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class MultipleOptionsLinkedQuestionAnswered : QuestionAnswered
    {
        public decimal[][] SelectedRosterVectors { get; private set; }

        public MultipleOptionsLinkedQuestionAnswered(Guid userId, Guid questionId, decimal[] rosterVector, 
            DateTimeOffset originDate, decimal[][] selectedRosterVectors, DateTime? answerTimeUtc = null)
            : base(userId, questionId, rosterVector, originDate, answerTimeUtc)
        {
            this.SelectedRosterVectors = selectedRosterVectors;
        }
    }
}
