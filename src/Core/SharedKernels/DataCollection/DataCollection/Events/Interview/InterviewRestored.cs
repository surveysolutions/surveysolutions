using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewRestored : InterviewActiveEvent
    {
        public InterviewRestored(Guid userId, DateTimeOffset originDate)
            : base(userId, originDate) {}
    }
}
