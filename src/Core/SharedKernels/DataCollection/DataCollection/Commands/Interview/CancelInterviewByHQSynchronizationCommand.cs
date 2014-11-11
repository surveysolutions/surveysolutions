using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class CancelInterviewByHQSynchronizationCommand : InterviewCommand
    {
        public CancelInterviewByHQSynchronizationCommand(Guid interviewId, Guid userId)
            : base(interviewId, userId) {}
    }
}