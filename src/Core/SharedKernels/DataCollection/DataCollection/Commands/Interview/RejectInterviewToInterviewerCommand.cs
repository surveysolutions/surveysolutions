using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class RejectInterviewToInterviewerCommand : InterviewCommand
    {
        public RejectInterviewToInterviewerCommand(Guid userId, Guid interviewId, Guid interviewerId, string comment, DateTime rejectTime)
            : base(interviewId, userId)
        {
            this.Comment = comment;
            this.RejectTime = rejectTime;
            this.InterviewerId = interviewerId;
        }

        public string Comment { get; set; }
        public DateTime RejectTime { get; set; }
        public Guid InterviewerId { get; set; }
    }
}