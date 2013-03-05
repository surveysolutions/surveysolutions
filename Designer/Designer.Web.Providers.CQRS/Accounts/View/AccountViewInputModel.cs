namespace Designer.Web.Providers.CQRS.Accounts.View
{
    public class AccountViewInputModel
    {
        public AccountViewInputModel(object providerUserKey)
        {
            ProviderUserKey = providerUserKey;
        }

        public AccountViewInputModel(string accountName, string accountEmail, string confirmationToken)
        {
            AccountName = accountName;
            AccountEmail = accountEmail;
            ConfirmationToken = confirmationToken;
        }

        /// <summary>
        /// Account Id
        /// </summary>
        public object ProviderUserKey { get; protected set; }
        /// <summary>
        /// Account name
        /// </summary>
        public string AccountName { get; protected set; }
        /// <summary>
        /// Account email
        /// </summary>
        public string AccountEmail { get; protected set; }
        /// <summary>
        /// Account confirmation token
        /// </summary>
        public string ConfirmationToken { get; protected set; }
    }
}
