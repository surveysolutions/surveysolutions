using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewDeclaredInvalid : InterviewPassiveEvent {
        public InterviewDeclaredInvalid(DateTimeOffset originDate) : base(originDate)
        {
        }
    }
}
