using System;

namespace WB.Core.SharedKernels.DataCollection.Commands.Assignment
{
    public class ReassignAssignment : AssignmentCommand
    {
        public Guid ResponsibleId { get; }
        public string Comment { get; }

        public ReassignAssignment(Guid assignmentId, Guid userId, Guid responsibleId, string comment) : base(assignmentId, userId)
        {
            ResponsibleId = responsibleId;
            Comment = comment;
        }
    }
}
