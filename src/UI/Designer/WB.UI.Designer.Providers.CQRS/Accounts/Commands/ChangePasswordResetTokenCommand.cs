namespace WB.UI.Designer.Providers.CQRS.Accounts.Commands
{
    using System;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    [Serializable]
    [MapsToAggregateRootMethod(typeof (AccountAR), "ChangePasswordResetToken")]
    public class ChangePasswordResetTokenCommand : AccountCommandBase
    {
        public ChangePasswordResetTokenCommand(
            Guid accountId, string passwordResetToken, DateTime passwordResetExpirationDate)
            : base(accountId)
        {
            PasswordResetToken = passwordResetToken;
            PasswordResetExpirationDate = passwordResetExpirationDate;
        }

        public DateTime PasswordResetExpirationDate { get; private set; }
        public string PasswordResetToken { get; private set; }
    }
}