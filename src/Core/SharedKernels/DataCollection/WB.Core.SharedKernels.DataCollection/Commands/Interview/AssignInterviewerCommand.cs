using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootMethod(typeof(Implementation.Aggregates.Interview), "AssignInterviewer")]
    public class AssignInterviewerCommand : InterviewCommand
    {
        public Guid InterviewerId { get; private set; }

        public AssignInterviewerCommand(Guid interviewId, Guid userId, Guid interviewerId)
            : base(interviewId, userId)
        {
            this.InterviewerId = interviewerId;
        }
    }
}