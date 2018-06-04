using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class RestoreInterviewCommand : InterviewCommand
    {
        public RestoreInterviewCommand(Guid interviewId, Guid userId)
            : base(interviewId, userId) {}
    }
}
