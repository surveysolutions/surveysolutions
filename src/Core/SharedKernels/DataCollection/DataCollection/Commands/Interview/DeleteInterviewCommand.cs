using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class DeleteInterviewCommand : InterviewCommand
    {
        public DeleteInterviewCommand(Guid interviewId, Guid userId)
            : base(interviewId, userId) {}
    }
}
