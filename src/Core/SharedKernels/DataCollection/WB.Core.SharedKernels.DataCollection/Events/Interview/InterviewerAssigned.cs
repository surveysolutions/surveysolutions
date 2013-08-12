using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewerAssigned : InterviewActiveEvent
    {
        public Guid InterviewerId { get; private set; }

        public InterviewerAssigned(Guid userId, Guid interviewerId)
            : base(userId)
        {
            this.InterviewerId = interviewerId;
        }
    }
}