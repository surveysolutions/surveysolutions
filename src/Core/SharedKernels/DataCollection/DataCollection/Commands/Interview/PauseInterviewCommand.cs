using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public abstract class TimestampedInterviewCommand : InterviewCommand
    {
        protected TimestampedInterviewCommand(Guid interviewId, Guid userId, DateTime localTime, DateTime utcTime) : base(interviewId, userId)
        {
            LocalTime = localTime;
            UtcTime = utcTime;
        }

        public DateTime LocalTime { get; set; }
        public DateTime UtcTime { get; set; }
    }

    public class PauseInterviewCommand : TimestampedInterviewCommand
    {
        public PauseInterviewCommand(Guid interviewId, Guid userId, DateTime localTime, DateTime utcTime) : base(interviewId, userId, localTime, utcTime)
        {
            LocalTime = localTime;
        }
    }

    public class ResumeInterviewCommand : TimestampedInterviewCommand
    {
        public ResumeInterviewCommand(Guid interviewId, Guid userId, DateTime localTime, DateTime utcTime) : base(interviewId, userId, localTime, utcTime)
        {
        }
    }

    public class CloseInterviewBySupervisorCommand : TimestampedInterviewCommand
    {
        public CloseInterviewBySupervisorCommand(Guid interviewId, Guid userId, DateTime localTime) : base(interviewId, userId, localTime, DateTime.UtcNow)
        {
        }
    }

    public class OpenInterviewBySupervisorCommand : TimestampedInterviewCommand
    {
        public OpenInterviewBySupervisorCommand(Guid interviewId, Guid userId, DateTime localTime) : base(interviewId, userId, localTime, DateTime.UtcNow)
        {
        }
    }
}