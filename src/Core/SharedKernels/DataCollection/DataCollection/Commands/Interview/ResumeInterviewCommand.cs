using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class ResumeInterviewCommand : TimestampedInterviewCommand
    {
        public AgentDeviceType DeviceType { get; }

        public ResumeInterviewCommand(Guid interviewId, Guid userId, AgentDeviceType deviceType) : base(interviewId, userId)
        {
            DeviceType = deviceType;
        }
    }
}
