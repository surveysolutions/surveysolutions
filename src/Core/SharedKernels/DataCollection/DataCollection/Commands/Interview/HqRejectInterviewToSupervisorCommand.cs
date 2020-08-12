using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class HqRejectInterviewToSupervisorCommand : InterviewCommand
    {
        public HqRejectInterviewToSupervisorCommand(Guid interviewId, Guid userId, Guid supervisorId, string comment)
            : base(interviewId, userId)
        {
            SupervisorId = supervisorId;
            Comment = comment;
        }

        public Guid SupervisorId { get; }
        public string Comment { get; set; }
    }
}
