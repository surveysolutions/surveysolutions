using System;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewSynchronized : InterviewActiveEvent
    {
        public InterviewSynchronized(InterviewSynchronizationDto interviewData, DateTimeOffset originDate)
            : base(interviewData.UserId, originDate)
        {
            InterviewData = interviewData;
        }

        public InterviewSynchronizationDto InterviewData { get; private set; }
    }
}
