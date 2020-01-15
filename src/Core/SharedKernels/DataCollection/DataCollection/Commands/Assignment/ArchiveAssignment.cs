using System;

namespace WB.Core.SharedKernels.DataCollection.Commands.Assignment
{
    public class ArchiveAssignment : AssignmentCommand
    {
        public ArchiveAssignment(Guid publicKey, Guid userId) : base(publicKey, userId)
        {
        }
    }
}
