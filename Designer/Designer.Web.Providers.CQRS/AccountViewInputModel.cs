using System;

namespace Designer.Web.Providers.CQRS
{
    public class AccountViewInputModel
    {
        public AccountViewInputModel(Guid accountId)
        {
            AccountId = accountId;
        }

        public AccountViewInputModel(string accountName, string accountEmail, string confirmationToken)
        {
            AccountId = Guid.Empty;
            AccountName = accountName;
            AccountEmail = accountEmail;
            ConfirmationToken = confirmationToken;
        }

        /// <summary>
        /// Account Id
        /// </summary>
        public Guid AccountId { get; protected set; }
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
