using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewerAssigned : InterviewActiveEvent
    {
        public Guid? InterviewerId { get; private set; }
        public DateTime? AssignTime { get; set; }
        public InterviewerAssigned(Guid userId, Guid? interviewerId, DateTimeOffset originDate)
            : base(userId, originDate)
        {
            this.InterviewerId = interviewerId;
            
            if (originDate != default(DateTimeOffset))
                this.AssignTime = originDate.UtcDateTime;
        }
    }
}
