using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class SupervisorAssigned : InterviewActiveEvent
    {
        public Guid SupervisorId { get; }
        public DateTime? AssignTime { get; }

        public SupervisorAssigned(Guid userId, Guid supervisorId, DateTimeOffset originDate)
            : base(userId, originDate)
        {
            this.SupervisorId = supervisorId;
            this.AssignTime = originDate.UtcDateTime;
        }
    }
}
