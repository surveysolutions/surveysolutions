using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Base
{
    public abstract class QuestionPassiveEvent : InterviewPassiveEvent
    {
        public Guid QuestionId { get; private set; }
        public decimal[] RosterVector { get; private set; }

        protected QuestionPassiveEvent(Guid questionId, decimal[] rosterVector, DateTimeOffset originDate) 
            : base (originDate)
        {
            this.QuestionId = questionId;
            this.RosterVector = rosterVector;
        }
    }
}
