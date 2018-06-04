using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewClosedBySupervisor : InterviewActiveEvent
    {
        public InterviewClosedBySupervisor(Guid userId, DateTimeOffset originDate) : base(userId, originDate)
        {
            //?check why do we need local
            this.LocalTime = originDate.LocalDateTime;
        }

        public DateTime? LocalTime { get; set; }
    }
}
