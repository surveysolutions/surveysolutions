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
        public DateTime UtcTime { get; }
    }
}