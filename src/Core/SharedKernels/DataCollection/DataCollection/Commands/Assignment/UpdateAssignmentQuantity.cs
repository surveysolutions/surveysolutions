using System;

namespace WB.Core.SharedKernels.DataCollection.Commands.Assignment
{
    public class UpdateAssignmentQuantity : AssignmentCommand
    {
        public int? Quantity { get; }

        public UpdateAssignmentQuantity(Guid publicKey, Guid userId, int? quantity) : base(publicKey, userId)
        {
            Quantity = quantity;
        }
    }
}
