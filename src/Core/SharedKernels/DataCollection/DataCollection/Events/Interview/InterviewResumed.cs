using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewResumed : InterviewActiveEvent
    {
        public InterviewResumed(Guid userId, DateTimeOffset originDate, DateTime? localTime = null, DateTime? utcTime = null) 
            : base(userId, originDate)
        {
            if (originDate != default(DateTimeOffset))
            {
                this.LocalTime = originDate.LocalDateTime;
                this.UtcTime = originDate.UtcDateTime;
            }
            else
            {
                if (localTime != null && localTime != default(DateTime))
                {
                    this.LocalTime = localTime;
                }

                if (utcTime != null && utcTime != default(DateTime))
                {
                    this.UtcTime = utcTime;
                }
            }
        }

        public DateTime? LocalTime { get; set; }
        public DateTime? UtcTime { get; set; }
    }
}
