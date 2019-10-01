using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Assignment
{
    public class AssignmentReceivedByTablet : AssignmentEvent
    {
        public AssignmentReceivedByTablet(Guid userId, DateTimeOffset originDate) : base(userId, originDate)
        {
        }
    }
}
