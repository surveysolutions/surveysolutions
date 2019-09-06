using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Assignment
{
    public class AssignmentMarkAsReceivedByTablet : AssignmentEvent
    {
        public AssignmentMarkAsReceivedByTablet(Guid userId, DateTimeOffset originDate) : base(userId, originDate)
        {
        }
    }
}
