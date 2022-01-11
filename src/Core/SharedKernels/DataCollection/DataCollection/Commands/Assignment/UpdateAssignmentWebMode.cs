using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Commands.Assignment
{
    public class UpdateAssignmentWebMode : AssignmentCommand
    {
        public bool WebMode { get; }
        public UpdateAssignmentWebMode(Guid publicKey, Guid userId, bool webMode, QuestionnaireIdentity questionnaireIdentity) 
            : base(publicKey, userId, questionnaireIdentity)
        {
            WebMode = webMode;
        }
    }
}
