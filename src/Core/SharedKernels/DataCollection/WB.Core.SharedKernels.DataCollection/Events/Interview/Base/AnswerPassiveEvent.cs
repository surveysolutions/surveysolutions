using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Base
{
    public abstract class AnswerPassiveEvent : InterviewPassiveEvent
    {
        public Guid QuestionId { get; private set; }

        protected AnswerPassiveEvent(Guid questionId)
        {
            this.QuestionId = questionId;
        }
    }
}