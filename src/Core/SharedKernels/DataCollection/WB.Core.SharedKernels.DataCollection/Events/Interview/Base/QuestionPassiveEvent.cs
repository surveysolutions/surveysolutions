using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Base
{
    public abstract class QuestionPassiveEvent : InterviewPassiveEvent
    {
        public Guid QuestionId { get; private set; }

        protected QuestionPassiveEvent(Guid questionId)
        {
            this.QuestionId = questionId;
        }
    }
}