using System;

namespace WB.Core.SharedKernels.DataCollection.Commands.Assignment
{
    public class UnarchiveAssignment : AssignmentCommand
    {
        public UnarchiveAssignment(Guid publicKey, Guid userId) : base(publicKey, userId)
        {
        }
    }
}
