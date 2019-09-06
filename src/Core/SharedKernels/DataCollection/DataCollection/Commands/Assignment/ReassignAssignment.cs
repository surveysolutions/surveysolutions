using System;

namespace WB.Core.SharedKernels.DataCollection.Commands.Assignment
{
    public class ReassignAssignment : AssignmentCommand
    {
        public Guid ResponsibleId { get; }

        public ReassignAssignment(Guid assignmentId, Guid userId, Guid responsibleId) : base(assignmentId, userId)
        {
            ResponsibleId = responsibleId;
        }
    }
}
