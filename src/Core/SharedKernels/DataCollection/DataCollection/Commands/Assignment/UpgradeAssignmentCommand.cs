using System;

namespace WB.Core.SharedKernels.DataCollection.Commands.Assignment
{
    public class UpgradeAssignmentCommand : AssignmentCommand
    {
        public UpgradeAssignmentCommand(Guid publicKey, Guid userId) : base(publicKey, userId)
        {
        }
    }
}
