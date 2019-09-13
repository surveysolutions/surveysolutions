using System;
using Ncqrs.Commanding;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class ReevaluateInterview : CommandBase
    {
        public ReevaluateInterview(Guid interviewId, Guid userId)
            : base(interviewId)
        {
            this.InterviewId = interviewId;
            this.UserId = userId;
        }

        public Guid InterviewId { get; private set; }
        public Guid UserId { get; private set; }
    }
}
