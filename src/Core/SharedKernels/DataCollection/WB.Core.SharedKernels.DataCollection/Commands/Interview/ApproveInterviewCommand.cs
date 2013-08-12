using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootMethod(typeof (Implementation.Aggregates.Interview), "Approve")]
    public class ApproveInterviewCommand : InterviewCommand
    {
        public ApproveInterviewCommand(Guid interviewId, Guid userId)
            : base(interviewId, userId) {}
    }
}