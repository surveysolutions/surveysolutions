using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class MoveInterviewToTeam : InterviewCommand
    {
        public Guid? InterviewerId { get; private set; }
        public Guid SupervisorId { get; private set; }
        public DateTime MoveUtcTime { get; private set; }
        
        public MoveInterviewToTeam(Guid interviewId, Guid userId, Guid supervisorId, Guid? interviewerId, DateTime moveUtcTime)
            : base(interviewId, userId)
        {
            this.MoveUtcTime = moveUtcTime;
            this.InterviewerId = interviewerId;
            this.SupervisorId = supervisorId;
        }
    }
}