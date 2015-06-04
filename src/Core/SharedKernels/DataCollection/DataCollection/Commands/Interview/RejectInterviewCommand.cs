using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class RejectInterviewCommand : InterviewCommand
    {
        public RejectInterviewCommand(Guid interviewId, Guid userId, string comment, DateTime rejectTime)
            : base(interviewId, userId)
        {
            this.Comment = comment;
            this.RejectTime = rejectTime;
        }

        public string Comment { get; set; }
        public DateTime RejectTime { get; set; }
    }
}