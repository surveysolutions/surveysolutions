using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Assignment
{
    public class AssignmentTargetAreaChanged : AssignmentEvent
    {
        public string TargetAreaName { get; }

        public AssignmentTargetAreaChanged(Guid userId, DateTimeOffset originDate, string targetAreaName) : base(userId, originDate)
        {
            TargetAreaName = targetAreaName;
        }
    }
}
