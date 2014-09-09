using System;
using System.Diagnostics;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewCompleted : InterviewActiveEvent
    {
        public InterviewCompleted(Guid userId, DateTime? completeTime)
            : base(userId)
        {
            if (completeTime != default(DateTime))
            {
                this.CompleteTime = completeTime;
            }
        }

        public DateTime? CompleteTime { get; private set; }
    }
}