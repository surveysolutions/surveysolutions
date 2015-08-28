using System;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Events
{
    [Obsolete("Use UserLoggedIn event instead of this")]
    //survived during clean up. db could contain that event
    public class AccountValidated
    {
        public DateTime LastLoginAt { get; set; }
    }
}