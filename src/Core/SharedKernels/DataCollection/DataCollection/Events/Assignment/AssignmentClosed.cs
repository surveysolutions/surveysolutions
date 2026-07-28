using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Assignment
{
    public class AssignmentClosed : AssignmentEvent
    {
        public string Comment { get; }

        public AssignmentClosed(Guid userId, DateTimeOffset originDate, string comment = null)
            : base(userId, originDate)
        {
            Comment = comment;
        }
    }
}
