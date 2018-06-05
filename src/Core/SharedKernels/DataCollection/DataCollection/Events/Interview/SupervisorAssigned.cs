using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class SupervisorAssigned : InterviewActiveEvent
    {
        public Guid SupervisorId { get; }
        public DateTime? AssignTime { get; }

        public SupervisorAssigned(Guid userId, Guid supervisorId, DateTimeOffset originDate, DateTime? assignTime = null)
            : base(userId, originDate)
        {
            this.SupervisorId = supervisorId;

            if (originDate != default(DateTimeOffset))
                this.AssignTime = originDate.UtcDateTime;
            else if (assignTime != null && assignTime != default(DateTime))
                this.AssignTime = assignTime;
        }
    }
}
