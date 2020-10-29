using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewResumed : InterviewActiveEvent
    {
        public InterviewResumed(Guid userId, DateTimeOffset originDate, AgentDeviceType deviceType) 
            : base(userId, originDate)
        {
            DeviceType = deviceType;
            if (originDate != default)
            {
                this.LocalTime = originDate.LocalDateTime;
                this.UtcTime = originDate.UtcDateTime;
            }
        }

        public AgentDeviceType DeviceType { get; set; }

        public DateTime? LocalTime { get; set; }
        public DateTime? UtcTime { get; set; }
    }
}
