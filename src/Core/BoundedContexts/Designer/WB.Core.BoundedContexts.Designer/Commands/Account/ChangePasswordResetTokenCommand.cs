using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (AccountAR), "ChangePasswordResetToken")]
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