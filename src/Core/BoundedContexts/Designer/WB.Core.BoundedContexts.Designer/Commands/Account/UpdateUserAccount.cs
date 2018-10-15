using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
    public class UpdateUserAccount : UserCommand
    {
        public UpdateUserAccount(Guid userId, string userName, bool isLockedOut, string passwordQuestion,
            string email, bool isConfirmed, string comment, bool canImportOnHq, string fullName)
            : base(userId)
        {
            this.UserName = userName;
            this.IsLockedOut = isLockedOut;
            this.PasswordQuestion = passwordQuestion;
            this.Email = email;
            this.IsConfirmed = isConfirmed;
            this.Comment = comment;
            this.CanImportOnHq = canImportOnHq;
            this.FullName = fullName;
        }

        public bool CanImportOnHq { get; set; }

        public string UserName { get; private set; }
        public bool IsLockedOut { get; private set; }
        public string PasswordQuestion { get; private set; }
        public string Email { get; private set; }
        public bool IsConfirmed { get; private set; }
        public string Comment { get; private set; }
        public string FullName { get; private set; }
    }
}
