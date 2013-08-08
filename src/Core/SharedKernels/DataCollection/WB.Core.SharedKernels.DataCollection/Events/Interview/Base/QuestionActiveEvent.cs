using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Base
{
    public abstract class QuestionActiveEvent : InterviewActiveEvent
    {
        public Guid QuestionId { get; private set; }

        protected QuestionActiveEvent(Guid userId, Guid questionId)
            : base(userId)
        {
            this.QuestionId = questionId;
        }
    }
}