using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewPaused : InterviewActiveEvent
    {
        public InterviewPaused(Guid userId, DateTime localTime, DateTime utcTime) : base(userId)
        {
            LocalTime = localTime;
            UtcTime = utcTime;
        }

        public DateTime LocalTime { get; set; }
        public DateTime UtcTime { get; set; }
    }
}