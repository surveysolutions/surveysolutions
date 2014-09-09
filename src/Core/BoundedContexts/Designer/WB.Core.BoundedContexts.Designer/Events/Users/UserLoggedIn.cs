using System;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Events
{
    public class UserLoggedIn
    {
        public DateTime LastLoginAt { get; set; }
    }
}
