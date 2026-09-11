using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Assignment
{
    public class AssignmentCompleted : AssignmentEvent
    {
        public string Comment { get; }

        public AssignmentCompleted(Guid userId, DateTimeOffset originDate, string comment = null)
            : base(userId, originDate)
        {
            Comment = comment;
        }
    }
}
