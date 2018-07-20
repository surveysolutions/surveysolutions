using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
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
