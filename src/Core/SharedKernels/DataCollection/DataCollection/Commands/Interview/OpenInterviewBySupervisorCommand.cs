using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class OpenInterviewBySupervisorCommand : ResumeInterviewCommand
    {
        public OpenInterviewBySupervisorCommand(Guid interviewId, Guid userId, AgentDeviceType deviceType) : base(interviewId, userId, deviceType)
        {
        }
    }
}
