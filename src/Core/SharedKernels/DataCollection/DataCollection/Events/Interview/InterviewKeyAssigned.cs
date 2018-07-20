using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewKeyAssigned : InterviewPassiveEvent
    {
        public InterviewKeyAssigned(InterviewKey key, DateTimeOffset originDate): base(originDate)
        {
            this.Key = key;
        }

        public InterviewKey Key { get;}
    }
}
