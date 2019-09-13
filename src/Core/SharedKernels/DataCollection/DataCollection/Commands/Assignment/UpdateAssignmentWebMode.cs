using System;

namespace WB.Core.SharedKernels.DataCollection.Commands.Assignment
{
    public class UpdateAssignmentWebMode : AssignmentCommand
    {
        public bool WebMode { get; }
        public UpdateAssignmentWebMode(Guid assignmentId, Guid userId, bool webMode) : base(assignmentId, userId)
        {
            WebMode = webMode;
        }
    }
}
