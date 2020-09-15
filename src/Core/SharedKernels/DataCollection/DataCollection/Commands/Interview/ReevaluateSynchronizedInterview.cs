using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class ReevaluateInterview : InterviewCommand
    {
        public ReevaluateInterview(Guid interviewId, Guid userId, DateTimeOffset? originDate = null): base(interviewId, userId)
        {
            this.InterviewId = interviewId;
            this.UserId = userId;
            this.OriginDate = originDate ?? DateTimeOffset.Now;
        }
    }
}
