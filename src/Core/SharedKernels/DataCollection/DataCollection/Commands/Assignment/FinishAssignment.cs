using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Commands.Assignment
{
    public class FinishAssignment : AssignmentCommand
    {
        public string Comment { get; }

        public FinishAssignment(Guid publicKey, Guid userId, QuestionnaireIdentity questionnaireIdentity, string comment = null)
            : base(publicKey, userId, questionnaireIdentity)
        {
            Comment = comment;
        }
    }
}
