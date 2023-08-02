using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewReceivedByInterviewer : InterviewPassiveEvent
    {
        public string DeviceId { get; set; }
        public InterviewReceivedByInterviewer(string deviceId, DateTimeOffset originDate) : base(originDate)
        {
            this.DeviceId = deviceId;
        }
    }
}
