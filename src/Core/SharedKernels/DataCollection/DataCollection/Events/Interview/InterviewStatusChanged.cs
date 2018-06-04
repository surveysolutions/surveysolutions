using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewStatusChanged : InterviewPassiveEvent
    {
        public  InterviewStatus? PreviousStatus { get; private set; }
        public InterviewStatus Status { get; private set; }
        public string Comment { get; private set; }
        public DateTime? UtcTime { get; private set; }

        public InterviewStatusChanged(InterviewStatus status, string comment, DateTimeOffset originDate, 
            InterviewStatus? previousStatus = null): base (originDate)
        {
            this.PreviousStatus = previousStatus;
            this.Status = status;
            this.Comment = comment;
        }
    }
}
