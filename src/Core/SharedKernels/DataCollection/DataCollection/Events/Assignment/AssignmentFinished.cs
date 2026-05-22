using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Assignment
{
    public class AssignmentFinished : AssignmentEvent
    {
        public string Comment { get; }

        public AssignmentFinished(Guid userId, DateTimeOffset originDate, string comment = null)
            : base(userId, originDate)
        {
            Comment = comment;
        }
    }
}
