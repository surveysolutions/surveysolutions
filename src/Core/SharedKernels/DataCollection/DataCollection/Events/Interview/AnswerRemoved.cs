using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    [Obsolete("Since v6.0")]
    public class AnswerRemoved : QuestionActiveEvent
    {
        public DateTime RemoveTimeUtc { get; private set; }

        public AnswerRemoved(Guid userId, Guid questionId, decimal[] rosterVector, DateTime removeTimeUtc)
            : base(userId, questionId, rosterVector)
        {
            this.RemoveTimeUtc = removeTimeUtc;
        }
    }
}