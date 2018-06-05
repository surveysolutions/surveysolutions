using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewRejected : InterviewActiveEvent
    {
        public string Comment { get; private set; }
        public DateTime? RejectTime { get; private set; }
        public InterviewRejected(Guid userId, string comment, DateTimeOffset originDate, DateTime? rejectTime = null)
            : base(userId, originDate)
        {
            this.Comment = comment;
            this.RejectTime = rejectTime;
        }
    }
}
