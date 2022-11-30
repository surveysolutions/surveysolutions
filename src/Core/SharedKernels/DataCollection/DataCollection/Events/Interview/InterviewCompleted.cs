using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewCompleted : InterviewActiveEvent
    {
        public InterviewCompleted(Guid userId, DateTimeOffset originDate, string comment)
            : base(userId, originDate)
        {
            this.Comment = comment;
        }

        [Obsolete("Please use OriginDate property")]
        public DateTime? CompleteTime { get; set; }
        public string Comment { get; private set; }
    }
}
