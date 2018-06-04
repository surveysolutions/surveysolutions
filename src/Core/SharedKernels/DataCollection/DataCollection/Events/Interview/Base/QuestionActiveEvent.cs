using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Base
{
    public abstract class QuestionActiveEvent : InterviewActiveEvent
    {
        public Guid QuestionId { get; private set; }
        public decimal[] RosterVector { get; private set; }

        protected QuestionActiveEvent(Guid userId, Guid questionId, decimal[] rosterVector, DateTimeOffset originDate)
            : base(userId, originDate)
        {
            this.QuestionId = questionId;
            this.RosterVector = rosterVector;
        }
    }
}
