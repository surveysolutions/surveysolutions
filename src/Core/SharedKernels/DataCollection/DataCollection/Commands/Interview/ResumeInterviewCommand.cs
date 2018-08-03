using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class ResumeInterviewCommand : TimestampedInterviewCommand
    {
        public ResumeInterviewCommand(Guid interviewId, Guid userId) : base(interviewId, userId)
        {
        }
    }
}
