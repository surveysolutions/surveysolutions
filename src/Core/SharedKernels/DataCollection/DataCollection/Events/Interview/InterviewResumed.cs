using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewResumed : InterviewActiveEvent
    {
        public InterviewResumed(Guid userId) : base(userId)
        {
        }

        public DateTime LocalTime { get; set; }
    }
}