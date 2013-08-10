using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewStatusChanged : InterviewActiveEvent
    {
        public InterviewStatus Status { get; private set; }

        public InterviewStatusChanged(Guid userId, InterviewStatus status)
            : base(userId)
        {
            this.Status = status;
        }
    }
}