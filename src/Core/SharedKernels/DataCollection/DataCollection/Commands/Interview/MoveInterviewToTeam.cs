using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class MoveInterviewToTeam : InterviewCommand
    {
        public Guid? InterviewerId { get; private set; }
        public Guid SupervisorId { get; private set; }
        
        public MoveInterviewToTeam(Guid interviewId, Guid userId, Guid supervisorId, Guid? interviewerId)
            : base(interviewId, userId)
        {
            this.InterviewerId = interviewerId;
            this.SupervisorId = supervisorId;
        }
    }
}
