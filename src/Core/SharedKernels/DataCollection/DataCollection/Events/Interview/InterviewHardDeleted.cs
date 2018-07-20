using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewHardDeleted : InterviewActiveEvent
    {
        public InterviewHardDeleted(Guid userId, DateTimeOffset originDate)
            : base(userId, originDate) {}
    }
}
