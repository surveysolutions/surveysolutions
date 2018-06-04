using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public abstract class TimestampedInterviewCommand : InterviewCommand
    {
        protected TimestampedInterviewCommand(Guid interviewId, Guid userId) 
            : base(interviewId, userId)
        {
        }

    }
}
