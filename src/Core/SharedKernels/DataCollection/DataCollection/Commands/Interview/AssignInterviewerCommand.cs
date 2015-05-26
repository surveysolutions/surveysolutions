using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class AssignInterviewerCommand : InterviewCommand
    {
        public Guid InterviewerId { get; set; }
        public DateTime AssignTime { get; set; }

        public AssignInterviewerCommand(Guid interviewId, Guid userId, Guid interviewerId, DateTime assignTime)
            : base(interviewId, userId)
        {
            if (interviewerId == Guid.Empty)
                throw new ArgumentException("Interviewer ID cannot be empty.");

            this.InterviewerId = interviewerId;
            this.AssignTime = assignTime;
        }
    }
}