using System;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Events
{
    [Obsolete("Use UserLoggedIn event instead of this")]
    public class AccountValidated
    {
        public DateTime LastLoginAt { get; set; }
    }
}