using System;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Events
{
    public class AccountLoginFailed
    {
        public DateTime FailedPasswordWindowStartedAt { get; set; }
    }
}
