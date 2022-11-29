using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Base
{
    public abstract class QuestionAnswered : QuestionActiveEvent
    {
        public DateTime? AnswerTimeUtc { get; set; }

        protected QuestionAnswered(Guid userId, Guid questionId, decimal[] rosterVector, DateTimeOffset originDate)
            : base(userId, questionId, rosterVector, originDate)
        {
        }
    }
}
