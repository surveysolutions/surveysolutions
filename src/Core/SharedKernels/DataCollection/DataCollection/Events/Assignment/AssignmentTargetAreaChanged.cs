using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Assignment
{
    public class AssignmentTargetAreaChanged : AssignmentEvent
    {
        public string TargetArea { get; }

        public AssignmentTargetAreaChanged(Guid userId, DateTimeOffset originDate, string targetArea) : base(userId, originDate)
        {
            TargetArea = targetArea;
        }
    }
}
