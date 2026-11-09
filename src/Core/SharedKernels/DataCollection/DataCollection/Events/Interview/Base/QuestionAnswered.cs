using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Base
{
    public abstract class QuestionAnswered : QuestionActiveEvent
    {
        [Obsolete("Please use OriginDate property")]
        public DateTime? AnswerTimeUtc { get; set; }

        /// <summary>
        /// Null means the rejection checkpoint is unavailable, Guid.Empty means no rejection was known,
        /// and any other value is the stable identifier of the rejection event.
        /// </summary>
        public Guid? LastRejectionEventIdentifier { get; set; }

        protected QuestionAnswered(Guid userId, Guid questionId, decimal[] rosterVector, DateTimeOffset originDate)
            : base(userId, questionId, rosterVector, originDate)
        {
        }
    }
}
