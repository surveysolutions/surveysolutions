using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class AssignResponsibleCommand : InterviewCommand
    {
        public Guid? SupervisorId { get; set; }
        public Guid? InterviewerId { get; set; }

        public AssignResponsibleCommand(Guid interviewId, Guid userId, Guid? interviewerId, Guid? supervisorId)
            : base(interviewId, userId)
        {
            this.InterviewerId = interviewerId;
            this.SupervisorId = supervisorId;
        }
    }
}
