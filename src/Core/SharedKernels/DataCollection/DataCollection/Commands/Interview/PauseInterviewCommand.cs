using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class PauseInterviewCommand : InterviewCommand
    {
        public PauseInterviewCommand(Guid interviewId, Guid userId, DateTime localTime) : base(interviewId, userId)
        {
            LocalTime = localTime;
        }

        public DateTime LocalTime { get; set; }
    }

    public class ResumeInterviewCommand : InterviewCommand
    {
        public ResumeInterviewCommand(Guid interviewId, Guid userId, DateTime localTime) : base(interviewId, userId)
        {
            LocalTime = localTime;
        }

        public DateTime LocalTime { get; set; }
    }

    public class CloseInterviewBySupervisorCommand : PauseInterviewCommand
    {
        public CloseInterviewBySupervisorCommand(Guid interviewId, Guid userId, DateTime localTime) : base(interviewId, userId, localTime)
        {
        }
    }

    public class OpenInterviewBySupervisorCommand : ResumeInterviewCommand
    {
        public OpenInterviewBySupervisorCommand(Guid interviewId, Guid userId, DateTime localTime) : base(interviewId, userId, localTime)
        {
        }
    }
}