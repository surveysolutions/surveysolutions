using System;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Events
{
    public class AccountValidated
    {
        public DateTime LastLoginAt { get; set; }
    }
}
