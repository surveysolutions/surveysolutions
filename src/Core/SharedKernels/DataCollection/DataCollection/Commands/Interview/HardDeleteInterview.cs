using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class HardDeleteInterview : InterviewCommand
    {
        public HardDeleteInterview(Guid interviewId, Guid userId)
            : base(interviewId, userId) {}
    }
}
