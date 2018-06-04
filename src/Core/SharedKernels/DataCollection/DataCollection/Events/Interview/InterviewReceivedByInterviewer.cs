using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewReceivedByInterviewer : InterviewPassiveEvent
    {
        public InterviewReceivedByInterviewer(DateTimeOffset originDate) : base(originDate)
        {
        }
    }
}
