using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Assignment
{
    public class AssignmentReopened : AssignmentEvent
    {
        public string Comment { get; }

        public AssignmentReopened(Guid userId, DateTimeOffset originDate, string comment = null)
            : base(userId, originDate)
        {
            Comment = comment;
        }
    }
}
