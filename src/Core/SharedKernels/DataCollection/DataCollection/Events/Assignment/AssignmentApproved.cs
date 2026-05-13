using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Assignment
{
    public class AssignmentApproved : AssignmentEvent
    {
        public string Comment { get; }

        public AssignmentApproved(Guid userId, DateTimeOffset originDate, string comment = null)
            : base(userId, originDate)
        {
            Comment = comment;
        }
    }
}
