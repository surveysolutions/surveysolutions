using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Assignment
{
    public class AssignmentSizeChanged : AssignmentEvent
    {
        public int Size { get; }

        public AssignmentSizeChanged(Guid userId, DateTimeOffset originDate, int size) : base(userId, originDate)
        {
            Size = size;
        }
    }
}
