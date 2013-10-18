using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview.Base
{
    public abstract class InterviewCommand : CommandBase
    {
        [AggregateRootId]
        public Guid InterviewId { get; private set; }

        private Guid userId;
        public Guid UserId
        {
            get
            {
                if (this.userId == Guid.Empty)
                    throw new ArgumentException("User ID cannot be empty.");

                return this.userId;
            }

            set { this.userId = value; }
        }

        protected InterviewCommand(Guid interviewId, Guid userId)
            : base(interviewId)
        {
            this.InterviewId = interviewId;
            this.UserId = userId;
        }
    }
}