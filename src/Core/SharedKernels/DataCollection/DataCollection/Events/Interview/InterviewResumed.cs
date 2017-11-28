using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewResumed : InterviewActiveEvent
    {
        public InterviewResumed(Guid userId, DateTime localTime, DateTime utcTime) : base(userId)
        {
            LocalTime = localTime;
            UtcTime = utcTime;
        }

        public DateTime LocalTime { get; set; }
        public DateTime UtcTime { get; set; }
    }
}