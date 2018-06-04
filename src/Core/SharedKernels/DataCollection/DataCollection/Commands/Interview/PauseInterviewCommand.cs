using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class PauseInterviewCommand : TimestampedInterviewCommand
    {
        public PauseInterviewCommand(Guid interviewId, Guid userId) 
            : base(interviewId, userId)
        {
        }
    }
}
