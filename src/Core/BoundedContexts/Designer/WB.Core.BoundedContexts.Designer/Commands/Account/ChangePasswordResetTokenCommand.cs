using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
    public class ChangePasswordResetTokenCommand : AccountCommandBase
    {
        public ChangePasswordResetTokenCommand(
            Guid accountId, string passwordResetToken, DateTime passwordResetExpirationDate)
            : base(accountId)
        {
            this.PasswordResetToken = passwordResetToken;
            this.PasswordResetExpirationDate = passwordResetExpirationDate;
        }

        public DateTime PasswordResetExpirationDate { get; private set; }
        public string PasswordResetToken { get; private set; }
    }
}