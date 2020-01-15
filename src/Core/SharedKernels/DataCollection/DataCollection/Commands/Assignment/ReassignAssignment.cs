using System;

namespace WB.Core.SharedKernels.DataCollection.Commands.Assignment
{
    public class ReassignAssignment : AssignmentCommand
    {
        public Guid ResponsibleId { get; }
        public string Comment { get; }

        public ReassignAssignment(Guid publicKey, Guid userId, Guid responsibleId, string comment) : base(publicKey, userId)
        {
            ResponsibleId = responsibleId;
            Comment = comment;
        }
    }
}
