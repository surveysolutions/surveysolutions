using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewStatusChanged : InterviewPassiveEvent
    {
        public InterviewStatus Status { get; private set; }

        public string Comment { get; private set; }

        public InterviewStatusChanged(InterviewStatus status, string comment = "")
        {
            this.Status = status;
            this.Comment = comment;
        }
    }
}