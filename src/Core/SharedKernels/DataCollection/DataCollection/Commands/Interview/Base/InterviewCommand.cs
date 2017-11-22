using System;
using Ncqrs.Commanding;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview.Base
{
    public abstract class InterviewCommand : CommandBase
    {
        public Guid InterviewId { get; private set; }
        public Guid UserId { get; set; }

        protected InterviewCommand(Guid interviewId, Guid? userId)
            : base(interviewId)
        {
            this.InterviewId = interviewId;
            this.UserId = userId ?? Guid.Empty;
        }
    }
}