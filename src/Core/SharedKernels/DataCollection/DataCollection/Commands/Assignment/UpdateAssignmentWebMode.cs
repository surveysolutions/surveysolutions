using System;

namespace WB.Core.SharedKernels.DataCollection.Commands.Assignment
{
    public class UpdateAssignmentWebMode : AssignmentCommand
    {
        public bool WebMode { get; }
        public UpdateAssignmentWebMode(Guid publicKey, Guid userId, bool webMode) : base(publicKey, userId)
        {
            WebMode = webMode;
        }
    }
}
