using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewCompleted : InterviewActiveEvent
    {
        public InterviewCompleted(Guid userId, DateTime completeTime)
            : base(userId)
        {
            this.CompleteTime = completeTime;
        }

        public DateTime CompleteTime { get; private set; }
    }
}