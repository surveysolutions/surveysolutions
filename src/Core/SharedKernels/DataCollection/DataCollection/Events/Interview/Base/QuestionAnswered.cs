using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Base
{
    public abstract class QuestionAnswered : QuestionActiveEvent
    {
        public DateTime? AnswerTimeUtc { get; private set; }

        protected QuestionAnswered(Guid userId, Guid questionId, decimal[] rosterVector, DateTimeOffset originDate, DateTime? answerTimeUtc = null)
            : base(userId, questionId, rosterVector, originDate)
        {
            if (originDate != default(DateTimeOffset))
            {
                this.AnswerTimeUtc = originDate.UtcDateTime;
            }
            else if (answerTimeUtc != default(DateTime))
            {
                this.AnswerTimeUtc = answerTimeUtc;
            }
        }
    }
}
