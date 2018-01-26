using System;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class CloseInterviewBySupervisorCommand : PauseInterviewCommand
    {
        public CloseInterviewBySupervisorCommand(Guid interviewId, Guid userId, DateTime localTime) : base(interviewId, userId, localTime, DateTime.UtcNow)
        {
        }
    }
}