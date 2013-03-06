using System;

namespace Designer.Web.Providers.CQRS.Accounts.Events
{
    public class AccountLocked
    {
        public DateTime LastLockedOutAt { get; set; }
    }
}
