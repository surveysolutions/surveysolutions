using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewStatusChanged : InterviewPassiveEvent
    {
        public InterviewStatus Status { get; private set; }

        public InterviewStatusChanged(InterviewStatus status)
        {
            this.Status = status;
        }
    }
}