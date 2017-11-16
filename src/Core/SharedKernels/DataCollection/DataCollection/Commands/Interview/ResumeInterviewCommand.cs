using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class ResumeInterviewCommand : InterviewCommand
    {
        public ResumeInterviewCommand(Guid interviewId, Guid userId, DateTime localTime) : base(interviewId, userId)
        {
            LocalTime = localTime;
        }

        public DateTime LocalTime { get; set; }
    }
}