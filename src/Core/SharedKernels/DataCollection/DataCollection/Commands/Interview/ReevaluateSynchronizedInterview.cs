using System;
using Ncqrs.Commanding;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class ReevaluateSynchronizedInterview : CommandBase
    {
        public ReevaluateSynchronizedInterview(Guid interviewId)
            : base(interviewId)
        {
            this.InterviewId = interviewId;
        }

        public Guid InterviewId { get; private set; }
    }
}
