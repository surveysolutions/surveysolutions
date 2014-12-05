using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class CancelInterviewByHqSynchronizationCommand : InterviewCommand
    {
        public CancelInterviewByHqSynchronizationCommand(Guid interviewId, Guid userId)
            : base(interviewId, userId) {}
    }
}