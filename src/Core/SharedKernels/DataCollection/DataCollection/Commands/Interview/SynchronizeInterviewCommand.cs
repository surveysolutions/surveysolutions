using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class SynchronizeInterviewCommand : InterviewCommand
    {
        public SynchronizeInterviewCommand(Guid interviewId, Guid userId, InterviewSynchronizationDto sycnhronizedInterview) : base(interviewId, userId)
        {
            SynchronizedInterview = sycnhronizedInterview;
        }

        public InterviewSynchronizationDto SynchronizedInterview { get; private set; }
    }
}
