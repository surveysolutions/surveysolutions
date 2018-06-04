using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class CreateInterviewFromSnapshotCommand : InterviewCommand
    {
        public CreateInterviewFromSnapshotCommand(Guid interviewId,
            Guid userId,
            InterviewSynchronizationDto sycnhronizedInterview) : base(interviewId, userId)
        {
            SynchronizedInterview = sycnhronizedInterview;
        }

        public InterviewSynchronizationDto SynchronizedInterview { get; private set; }
    }
}
