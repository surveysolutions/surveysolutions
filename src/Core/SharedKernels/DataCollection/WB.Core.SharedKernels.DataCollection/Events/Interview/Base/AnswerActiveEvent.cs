using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Base
{
    public abstract class AnswerActiveEvent : InterviewActiveEvent
    {
        public Guid QuestionId { get; private set; }

        protected AnswerActiveEvent(Guid userId, Guid questionId)
            : base(userId)
        {
            this.QuestionId = questionId;
        }
    }
}