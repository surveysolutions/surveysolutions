using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Commands.Assignment
{
    public class UpdateAssignmentQuantity : AssignmentCommand
    {
        public int? Quantity { get; }

        public UpdateAssignmentQuantity(Guid publicKey, Guid userId, int? quantity, QuestionnaireIdentity questionnaireIdentity) 
            : base(publicKey, userId, questionnaireIdentity)
        {
            Quantity = quantity;
        }
    }
}
