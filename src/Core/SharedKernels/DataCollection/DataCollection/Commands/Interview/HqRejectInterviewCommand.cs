using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class HqRejectInterviewCommand : InterviewCommand
    {
        public HqRejectInterviewCommand(Guid interviewId, Guid userId, string comment)
            : base(interviewId, userId)
        {
            this.Comment = comment;
        }

        public string Comment { get; set; }
    }
}
