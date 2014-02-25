using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class SupervisorAssigned : InterviewActiveEvent
    {
        public Guid SupervisorId { get; private set; }

        public SupervisorAssigned(Guid userId, Guid supervisorId)
            : base(userId)
        {
            this.SupervisorId = supervisorId;
        }
    }
}