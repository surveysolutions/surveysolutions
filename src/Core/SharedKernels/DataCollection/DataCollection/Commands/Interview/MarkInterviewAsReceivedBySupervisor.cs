using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class MarkInterviewAsReceivedBySupervisor : InterviewCommand
    {
        public MarkInterviewAsReceivedBySupervisor(Guid interviewId, Guid userId)
            : base(interviewId, userId)
        {
        }
    }
}
