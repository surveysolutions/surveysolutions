using System;

namespace WB.Core.SharedKernels.DataCollection.Commands.Assignment
{
    public class MarkAssignmentAsReceivedByTablet : AssignmentCommand
    {
        public MarkAssignmentAsReceivedByTablet(Guid publicKey, Guid userId) : base(publicKey, userId)
        {
        }
    }
}
