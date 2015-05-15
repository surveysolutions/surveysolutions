using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class ApproveInterviewCommand : InterviewCommand
    {
        public ApproveInterviewCommand(Guid interviewId, Guid userId, string comment, DateTime approveTime)
            : base(interviewId, userId)
        {
            this.Comment = comment;
            this.ApproveTime = approveTime;
        }

        public string Comment { get; set; }

        public DateTime ApproveTime { get; set; }
    }
}