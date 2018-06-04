using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class RejectInterviewToInterviewerCommand : InterviewCommand
    {
        public RejectInterviewToInterviewerCommand(Guid userId, Guid interviewId, Guid interviewerId, string comment)
            : base(interviewId, userId)
        {
            this.Comment = comment;
            this.InterviewerId = interviewerId;
        }

        public string Comment { get; set; }
        public Guid InterviewerId { get; set; }
    }
}
