using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewDeclaredValid : InterviewPassiveEvent {
        public InterviewDeclaredValid(DateTimeOffset originDate) : base(originDate)
        {
        }
    }
}
