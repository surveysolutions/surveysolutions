using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Commands.Assignment
{
    public class DeleteAssignment : AssignmentCommand
    {
        public DeleteAssignment(Guid publicKey, Guid userId, QuestionnaireIdentity questionnaireIdentity) 
            : base(publicKey, userId, questionnaireIdentity)
        {
        }
    }
}
