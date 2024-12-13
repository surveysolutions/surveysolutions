using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Commands.Assignment
{
    public class UpdateAssignmentTargetArea : AssignmentCommand
    {
        public string TargetAreaName { get; }

        public UpdateAssignmentTargetArea(Guid publicKey, Guid userId, string targetAreaName, QuestionnaireIdentity questionnaireIdentity) 
            : base(publicKey, userId, questionnaireIdentity)
        {
            TargetAreaName = targetAreaName;
        }
    }
}
