﻿using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    public class InterviewReceivedByInterviewerCommandValidator : ICommandValidator<StatefulInterview, InterviewCommand>
    {
        public void Validate(StatefulInterview aggregate, InterviewCommand command)
        {
            if (aggregate.ReceivedByInterviewer)
                throw new InterviewException(
                    WebInterviewResources.InterviewReceivedByInterviewer,
                    InterviewDomainExceptionType.InterviewRecievedByDevice)
                {
                    Data =
                    {
                        {InterviewQuestionInvariants.ExceptionKeys.InterviewId, aggregate.Id},
                    }
                };
        }
    }
}
