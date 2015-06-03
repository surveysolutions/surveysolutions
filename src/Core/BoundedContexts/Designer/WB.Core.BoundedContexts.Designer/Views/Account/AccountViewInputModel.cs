using System;

namespace WB.Core.BoundedContexts.Designer.Views.Account
{
    public class AccountViewInputModel
    {
        public AccountViewInputModel(Guid providerUserKey)
        {
            this.ProviderUserKey = providerUserKey;
        }

        public AccountViewInputModel(
            string accountName, string accountEmail, string confirmationToken, string resetPasswordToken)
        {
            this.AccountName = accountName;
            this.AccountEmail = accountEmail;
            this.ConfirmationToken = confirmationToken;
            this.ResetPasswordToken = resetPasswordToken;
        }

        public string AccountEmail { get; protected set; }

        public string AccountName { get; protected set; }

        public string ConfirmationToken { get; protected set; }

        public Guid? ProviderUserKey { get; protected set; }

        public string ResetPasswordToken { get; protected set; }
    }
}