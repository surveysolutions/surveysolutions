using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewRejected : InterviewActiveEvent
    {
        public InterviewRejected(Guid userId)
            : base(userId) {}
    }
}