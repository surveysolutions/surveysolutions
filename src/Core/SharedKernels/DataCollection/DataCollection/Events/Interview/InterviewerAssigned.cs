using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewerAssigned : InterviewActiveEvent
    {
        public Guid? InterviewerId { get; private set; }
        public DateTime? AssignTime { get; private set; }
        public InterviewerAssigned(Guid userId, Guid? interviewerId, DateTimeOffset originDate, DateTime? assignTime = null)
            : base(userId, originDate)
        {
            this.InterviewerId = interviewerId;

            if (assignTime != null && assignTime != default(DateTime))
            {
                this.AssignTime = assignTime;
            }
        }
    }
}
