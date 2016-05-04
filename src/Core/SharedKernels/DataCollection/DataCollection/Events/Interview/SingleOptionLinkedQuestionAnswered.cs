using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class SingleOptionLinkedQuestionAnswered : QuestionAnswered
    {
        public decimal[] SelectedRosterVector { get; private set; }

        public SingleOptionLinkedQuestionAnswered(Guid userId, Guid questionId, decimal[] rosterVector, DateTime answerTimeUtc, decimal[] selectedRosterVector)
            : base(userId, questionId, rosterVector, answerTimeUtc)
        {
            this.SelectedRosterVector = selectedRosterVector;
        }
    }
}