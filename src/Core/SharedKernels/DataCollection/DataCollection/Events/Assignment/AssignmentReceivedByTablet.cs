using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Assignment
{
    public class AssignmentReceivedByTablet : AssignmentEvent
    {
        public string DeviceId { set; get; }

        public AssignmentReceivedByTablet(Guid userId, string deviceId, DateTimeOffset originDate) : base(userId, originDate)
        {
            this.DeviceId = deviceId;
        }
    }
}
