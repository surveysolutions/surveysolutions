using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewReceivedBySupervisor : InterviewPassiveEvent
    {
        public InterviewReceivedBySupervisor(DateTimeOffset originDate) : base(originDate)
        {
        }
    }
}
