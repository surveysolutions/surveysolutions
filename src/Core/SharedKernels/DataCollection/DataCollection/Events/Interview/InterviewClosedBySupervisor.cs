using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewClosedBySupervisor : InterviewActiveEvent
    {
        public InterviewClosedBySupervisor(Guid userId, DateTimeOffset originDate, DateTime localTime) 
            : base(userId, originDate)
        {
            if (localTime != null && localTime != default(DateTime))
            {
                this.LocalTime = localTime;
            }
        }

        public DateTime? LocalTime { get; set; }
    }
}
