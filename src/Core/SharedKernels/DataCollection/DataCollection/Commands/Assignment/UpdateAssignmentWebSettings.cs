using System;

namespace WB.Core.SharedKernels.DataCollection.Commands.Assignment
{
    public class UpdateAssignmentWebSettings : AssignmentCommand
    {
        public bool? WebMode { get; }
        public string Email { get; }
        public string Password { get; }

        public UpdateAssignmentWebSettings(Guid assignmentId, Guid userId, bool? webMode, string email, string password) : base(assignmentId, userId)
        {
            WebMode = webMode;
            Email = email;
            Password = password;
        }
    }
}
