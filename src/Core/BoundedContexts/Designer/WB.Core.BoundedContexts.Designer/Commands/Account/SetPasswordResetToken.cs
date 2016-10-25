using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
    public class SetPasswordResetToken : UserCommand
    {
        public SetPasswordResetToken(
            Guid userId, string passwordResetToken, DateTime passwordResetExpirationDate)
            : base(userId)
        {
            this.PasswordResetToken = passwordResetToken;
            this.PasswordResetExpirationDate = passwordResetExpirationDate;
        }

        public DateTime PasswordResetExpirationDate { get; private set; }
        public string PasswordResetToken { get; private set; }
    }
}