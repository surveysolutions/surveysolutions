using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Commands.Assignment
{
    public class UpgradeAssignmentCommand : AssignmentCommand
    {
        public UpgradeAssignmentCommand(Guid publicKey, Guid userId, QuestionnaireIdentity questionnaireIdentity) 
            : base(publicKey, userId, questionnaireIdentity)
        {
        }
    }
}
