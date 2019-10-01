using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Assignment
{
    public class AssignmentArchived : AssignmentEvent
    {
        public AssignmentArchived(Guid userId, DateTimeOffset originDate) : base(userId, originDate)
        {
        }
    }
}
