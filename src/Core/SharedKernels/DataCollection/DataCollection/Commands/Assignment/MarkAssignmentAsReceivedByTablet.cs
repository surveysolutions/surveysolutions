using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Commands.Assignment
{
    public class MarkAssignmentAsReceivedByTablet : AssignmentCommand
    {
        public MarkAssignmentAsReceivedByTablet(Guid publicKey, Guid userId, QuestionnaireIdentity questionnaireIdentity) 
            : base(publicKey, userId, questionnaireIdentity)
        {
        }
    }
}
