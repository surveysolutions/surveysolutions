using System;

namespace WB.Core.SharedKernels.DataCollection.Commands.Assignment
{
    public class DeleteAssignment : AssignmentCommand
    {
        public DeleteAssignment(Guid publicKey, Guid userId) : base(publicKey, userId)
        {
        }
    }
}
