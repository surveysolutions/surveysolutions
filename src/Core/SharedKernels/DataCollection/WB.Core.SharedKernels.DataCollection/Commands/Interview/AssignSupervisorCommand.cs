using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootMethod(typeof(Implementation.Aggregates.Interview), "AssignSupervisor")]
    public class AssignSupervisorCommand : InterviewCommand
    {
        public Guid SupervisorId { get; private set; }

        public AssignSupervisorCommand(Guid interviewId, Guid userId, Guid supervisorId)
            : base(interviewId, userId)
        {
            this.SupervisorId = supervisorId;
        }
    }
}