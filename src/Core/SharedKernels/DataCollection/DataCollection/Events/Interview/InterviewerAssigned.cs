using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewerAssigned : InterviewActiveEvent
    {
        public Guid InterviewerId { get; private set; }
        public DateTime? AssignTime { get; private set; }
        public InterviewerAssigned(Guid userId, Guid interviewerId, DateTime? assignTime)
            : base(userId)
        {
            this.InterviewerId = interviewerId;
            AssignTime = assignTime;
        }
    }
}