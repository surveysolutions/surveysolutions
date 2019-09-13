using System;

namespace WB.Core.SharedKernels.DataCollection.Commands.Assignment
{
    public class UpdateAssignmentQuantity : AssignmentCommand
    {
        public int? Quantity { get; }

        public UpdateAssignmentQuantity(Guid assignmentId, Guid userId, int? quantity) : base(assignmentId, userId)
        {
            Quantity = quantity;
        }
    }
}
