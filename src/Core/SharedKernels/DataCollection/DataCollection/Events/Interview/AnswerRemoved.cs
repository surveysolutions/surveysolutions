using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    [Obsolete("Since v6.0")]
    public class AnswerRemoved : QuestionActiveEvent
    {
        [Obsolete("Please use OriginDate property")]
        public DateTime? RemoveTimeUtc { get; set; }

        public AnswerRemoved(Guid userId, Guid questionId, decimal[] rosterVector, DateTimeOffset originDate)
            : base(userId, questionId, rosterVector, originDate)
        {

        }
    }
}
