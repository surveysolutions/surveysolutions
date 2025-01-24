using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Commands.Assignment
{
    public class UpdateAssignmentTargetArea : AssignmentCommand
    {
        public string TargetArea { get; }

        public UpdateAssignmentTargetArea(Guid publicKey, Guid userId, string targetArea, QuestionnaireIdentity questionnaireIdentity) 
            : base(publicKey, userId, questionnaireIdentity)
        {
            TargetArea = targetArea;
        }
    }
}
