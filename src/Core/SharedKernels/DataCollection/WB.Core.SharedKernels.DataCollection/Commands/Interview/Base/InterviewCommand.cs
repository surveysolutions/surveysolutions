﻿using System;
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
            : base(interviewId)
        {
            ThrowArgumentExceptionIfGuidIsEmpty(userId);

            this.InterviewId = interviewId;
            this.UserId = userId;
        }

        protected void ThrowArgumentExceptionIfGuidIsEmpty(Guid guid)
        {
            if (guid == Guid.Empty)
                throw new ArgumentException("Guid cannot be empty.");
        }
    }
}