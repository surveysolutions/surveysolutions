using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview.Base
{
    public abstract class InterviewCommand : CommandBase
    {
        [AggregateRootId]
        public Guid InterviewId { get; private set; }

        public Guid UserId { get; private set; }

        protected InterviewCommand(Guid interviewId, Guid userId)
        {
            this.InterviewId = interviewId;
            this.UserId = userId;
        }
    }
}