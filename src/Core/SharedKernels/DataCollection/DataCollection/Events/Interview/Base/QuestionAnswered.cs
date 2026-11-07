using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Base
{
    public abstract class QuestionAnswered : QuestionActiveEvent
    {
        [Obsolete("Please use OriginDate property")]
        public DateTime? AnswerTimeUtc { get; set; }

        public int? LastRejectionEventSequence { get; set; }

        protected QuestionAnswered(Guid userId, Guid questionId, decimal[] rosterVector, DateTimeOffset originDate)
            : base(userId, questionId, rosterVector, originDate)
        {
        }
    }
}
