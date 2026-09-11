using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Commands.Assignment
{
    public class ReopenAssignment : AssignmentCommand
    {
        public string Comment { get; }

        public ReopenAssignment(Guid publicKey, Guid userId, QuestionnaireIdentity questionnaireIdentity, string comment = null)
            : base(publicKey, userId, questionnaireIdentity)
        {
            Comment = comment;
        }
    }
}
