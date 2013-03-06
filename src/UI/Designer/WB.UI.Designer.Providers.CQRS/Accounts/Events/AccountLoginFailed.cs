using System;

namespace Designer.Web.Providers.CQRS.Accounts.Events
{
    public class AccountLoginFailed
    {
        public DateTime FailedPasswordWindowStartedAt { get; set; }
    }
}
