using System;

namespace WB.Core.SharedKernels.DataCollection.Commands.Assignment
{
    public class DeleteAssignment : AssignmentCommand
    {
        public DeleteAssignment(Guid assignmentId, Guid userId) : base(assignmentId, userId)
        {
        }
    }
}
