using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewSentToHeadquarters : InterviewPassiveEvent {
        public InterviewSentToHeadquarters(DateTimeOffset originDate) : base(originDate)
        {
        }
    }
}
