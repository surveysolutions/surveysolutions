using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class MarkInterviewAsReceivedByInterviewer : InterviewCommand
    {
        public MarkInterviewAsReceivedByInterviewer(Guid interviewId, Guid userId)
            : base(interviewId, userId)
        {
        }
    }
}
