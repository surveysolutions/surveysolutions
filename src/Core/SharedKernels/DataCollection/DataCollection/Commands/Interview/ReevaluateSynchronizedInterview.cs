using System;
using Ncqrs.Commanding;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class ReevaluateSynchronizedInterview : CommandBase
    {
        public ReevaluateSynchronizedInterview(Guid interviewId, Guid userId)
            : base(interviewId)
        {
            this.InterviewId = interviewId;
            this.UserId = userId;
        }

        public Guid InterviewId { get; private set; }
        public Guid UserId { get; private set; }
    }
}
