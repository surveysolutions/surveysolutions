using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class PauseInterviewCommand : InterviewCommand
    {
        public PauseInterviewCommand(Guid interviewId, Guid userId, DateTime localTime, DateTime utcTime) : base(interviewId, userId)
        {
            LocalTime = localTime;
            UtcTime = utcTime;
        }

        public DateTime LocalTime { get; set; }

        public DateTime UtcTime { get; set; }
    }

    public class ResumeInterviewCommand : InterviewCommand
    {
        public ResumeInterviewCommand(Guid interviewId, Guid userId, DateTime localTime, DateTime utcTime) : base(interviewId, userId)
        {
            LocalTime = localTime;
            UtcTime = utcTime;
        }

        public DateTime UtcTime { get; set; }

        public DateTime LocalTime { get; set; }
    }

    public class CloseInterviewBySupervisorCommand : PauseInterviewCommand
    {
        public CloseInterviewBySupervisorCommand(Guid interviewId, Guid userId, DateTime localTime) : base(interviewId, userId, localTime, DateTime.UtcNow)
        {
        }
    }

    public class OpenInterviewBySupervisorCommand : ResumeInterviewCommand
    {
        public OpenInterviewBySupervisorCommand(Guid interviewId, Guid userId, DateTime localTime) : base(interviewId, userId, localTime, DateTime.UtcNow)
        {
        }
    }
}