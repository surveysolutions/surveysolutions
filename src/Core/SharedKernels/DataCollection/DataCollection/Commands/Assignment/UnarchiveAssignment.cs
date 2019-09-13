using System;

namespace WB.Core.SharedKernels.DataCollection.Commands.Assignment
{
    public class UnarchiveAssignment : AssignmentCommand
    {
        public UnarchiveAssignment(Guid assignmentId, Guid userId) : base(assignmentId, userId)
        {
        }
    }
}
