using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Assignment
{
    public class AssignmentUnarchived : AssignmentEvent
    {
        public AssignmentUnarchived(Guid userId, DateTimeOffset originDate) : base(userId, originDate)
        {
        }
    }
}
