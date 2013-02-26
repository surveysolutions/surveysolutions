using System;

namespace Designer.Web.Providers.Repositories.CQRS
{
    public class AccountViewInputModel
    {
        public AccountViewInputModel(Guid accountId)
        {
            AccountId = accountId;
        }

        public AccountViewInputModel(string accountName, string accountEmail)
        {
            AccountName = accountName;
            AccountEmail = accountEmail;
        }

        public Guid AccountId { get; protected set; }
        public string AccountName { get; protected set; }
        public string AccountEmail { get; protected set; }
    }
}
