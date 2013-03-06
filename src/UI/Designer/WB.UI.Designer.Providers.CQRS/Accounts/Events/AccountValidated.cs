using System;

namespace Designer.Web.Providers.CQRS.Accounts.Events
{
    public class AccountValidated
    {
        public DateTime LastLoginAt { get; set; }
    }
}
