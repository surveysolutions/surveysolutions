using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class PauseInterviewCommand : InterviewCommand
    {
        public PauseInterviewCommand(Guid interviewId, Guid userId) : base(interviewId, userId)
        {
        }
    }

    public class ResumeInterviewCommand : InterviewCommand
    {
        public ResumeInterviewCommand(Guid interviewId, Guid userId) : base(interviewId, userId)
        {
        }
    }
}