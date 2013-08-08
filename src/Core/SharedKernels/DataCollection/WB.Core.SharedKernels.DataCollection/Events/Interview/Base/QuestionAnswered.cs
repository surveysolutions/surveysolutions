using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Base
{
    public abstract class QuestionAnswered : QuestionActiveEvent
    {
        public DateTime AnswerTime { get; private set; }

        protected QuestionAnswered(Guid userId, Guid questionId, DateTime answerTime)
            : base(userId, questionId)
        {
            this.AnswerTime = answerTime;
        }
    }
}