using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Assignment
{
    public class AssignmentQuantityChanged : AssignmentEvent
    {
        public int? Quantity { get; }

        public AssignmentQuantityChanged(Guid userId, DateTimeOffset originDate, int? quantity) : base(userId, originDate)
        {
            Quantity = quantity;
        }
    }
}
