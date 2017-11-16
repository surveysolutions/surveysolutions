using System;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class OpenInterviewBySupervisorCommand : ResumeInterviewCommand
    {
        public OpenInterviewBySupervisorCommand(Guid interviewId, Guid userId, DateTime localTime) : base(interviewId, userId, localTime)
        {
        }
    }
}