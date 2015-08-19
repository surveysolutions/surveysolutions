using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class MultipleOptionsLinkedQuestionAnswered : QuestionAnswered
    {
        public decimal[][] SelectedRosterVectors { get; private set; }

        public MultipleOptionsLinkedQuestionAnswered(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTimeUtc, decimal[][] selectedRosterVectors)
            : base(userId, questionId, rosterVector, answerTimeUtc)
        {
            this.SelectedRosterVectors = selectedRosterVectors;
        }
    }
}
