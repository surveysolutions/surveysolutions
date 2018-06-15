using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewPaused : InterviewActiveEvent
    {
        public InterviewPaused(Guid userId, DateTimeOffset originDate) 
            : base(userId, originDate)
        {
            if (originDate != default(DateTimeOffset))
            {
                this.LocalTime = originDate.LocalDateTime;
                this.UtcTime = originDate.UtcDateTime;
            }
        }

        public DateTime? LocalTime { get; set; }
        public DateTime? UtcTime { get; set; }
    }
}
