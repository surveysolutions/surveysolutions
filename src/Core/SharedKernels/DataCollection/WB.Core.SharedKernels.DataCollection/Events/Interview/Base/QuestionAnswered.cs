using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Base
{
    public abstract class QuestionAnswered : InterviewEvent
    {
        public Guid QuestionId { get; private set; }
        public DateTime AnswerTime { get; private set; }

        protected QuestionAnswered(Guid userId, Guid questionId, DateTime answerTime)
            : base(userId)
        {
            this.QuestionId = questionId;
            this.AnswerTime = answerTime;
        }
    }
}