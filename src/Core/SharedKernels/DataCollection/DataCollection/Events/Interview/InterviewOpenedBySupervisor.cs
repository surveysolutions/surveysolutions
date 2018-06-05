using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewOpenedBySupervisor : InterviewActiveEvent
    {
        public InterviewOpenedBySupervisor(Guid userId, DateTimeOffset originDate, DateTime? localTime = null) : base(userId, originDate)
        {
            LocalTime = localTime;
        }
        public DateTime? LocalTime { get; set; }
    }
}
