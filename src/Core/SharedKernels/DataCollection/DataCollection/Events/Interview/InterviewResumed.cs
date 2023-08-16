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
        }

        public AgentDeviceType DeviceType { get; set; }

        [Obsolete("Please use OriginDate property")]
        public DateTime? LocalTime { get; set; }
        [Obsolete("Please use OriginDate property")]
        public DateTime? UtcTime { get; set; }
    }
}
