using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewSynchronized : InterviewActiveEvent
    {
        public InterviewSynchronized(InterviewSynchronizationDto interviewData)
            : base(interviewData.UserId)
        {
            InterviewData = interviewData;
        }

        public InterviewSynchronizationDto InterviewData { get; private set; }
    }
}
