using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewApproved : InterviewActiveEvent
    {
        public string Comment { get; private set; }
        public DateTime? ApproveTime { get; set; }
        public InterviewApproved(Guid userId, string comment, DateTimeOffset originDate) 
            : base(userId, originDate)
        {
            this.Comment = comment;

            if (originDate != default(DateTimeOffset))
                this.ApproveTime = originDate.UtcDateTime;
        }
    }
}
