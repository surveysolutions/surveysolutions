using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Commands.Assignment
{
    public class ReassignAssignment : AssignmentCommand
    {
        public Guid ResponsibleId { get; }
        public string Comment { get; }

        public ReassignAssignment(Guid publicKey, Guid userId, Guid responsibleId, string comment, QuestionnaireIdentity questionnaireIdentity) 
            : base(publicKey, userId, questionnaireIdentity)
        {
            ResponsibleId = responsibleId;
            Comment = comment;
        }
    }
}
