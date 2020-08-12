using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class HqRejectInterviewToInterviewerCommand : InterviewCommand
    {
        public HqRejectInterviewToInterviewerCommand(Guid interviewId, Guid userId, Guid interviewerId, Guid supervisorId, string comment)
            : base(interviewId, userId)
        {
            InterviewerId = interviewerId;
            SupervisorId = supervisorId;
            Comment = comment;
        }

        public Guid InterviewerId { get; }
        public Guid SupervisorId { get; set; }
        public string Comment { get; set; }
    }
}
