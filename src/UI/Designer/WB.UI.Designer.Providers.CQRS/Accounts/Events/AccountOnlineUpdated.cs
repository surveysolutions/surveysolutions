using System;

namespace Designer.Web.Providers.CQRS.Accounts.Events
{
    public class AccountOnlineUpdated
    {
        public DateTime LastActivityAt { get; set; }
    }
}
