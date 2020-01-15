using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Assignment
{
    public class AssignmentDeleted : AssignmentEvent
    {
        public AssignmentDeleted(Guid userId, DateTimeOffset originDate) : base(userId, originDate)
        {
        }
    }
}
