using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class MultipleOptionsLinkedQuestionAnswered : QuestionAnswered
    {
        public decimal[][] SelectedRosterVectors { get; private set; }

        public MultipleOptionsLinkedQuestionAnswered(Guid userId, Guid questionId, decimal[] rosterVector, 
            DateTimeOffset originDate, decimal[][] selectedRosterVectors)
            : base(userId, questionId, rosterVector, originDate)
        {
            this.SelectedRosterVectors = selectedRosterVectors;
        }
    }
}
