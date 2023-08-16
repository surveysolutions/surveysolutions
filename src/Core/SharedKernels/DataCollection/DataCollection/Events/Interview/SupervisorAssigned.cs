using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class SupervisorAssigned : InterviewActiveEvent
    {
        public Guid SupervisorId { get; }
        [Obsolete("Please use OriginDate property")]
        public DateTime? AssignTime { get; set; }

        public SupervisorAssigned(Guid userId, Guid supervisorId, DateTimeOffset originDate)
            : base(userId, originDate)
        {
            this.SupervisorId = supervisorId;
        }
    }
}
