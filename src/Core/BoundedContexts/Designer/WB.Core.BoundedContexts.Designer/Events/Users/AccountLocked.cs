using System;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Events
{
    public class AccountLocked
    {
        public DateTime LastLockedOutAt { get; set; }
    }
}
