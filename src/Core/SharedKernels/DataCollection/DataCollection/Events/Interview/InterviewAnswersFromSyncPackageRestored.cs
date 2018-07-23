using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.Enumerator.Events
{
    [Obsolete("Since v6.0")]
    public class InterviewAnswersFromSyncPackageRestored : InterviewActiveEvent
    {
        public InterviewAnswersFromSyncPackageRestored(Guid userId, DateTimeOffset originDate)
            : base(userId, originDate){}
    }
}
