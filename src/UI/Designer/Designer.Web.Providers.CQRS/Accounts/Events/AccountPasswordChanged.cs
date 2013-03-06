
using System;
namespace Designer.Web.Providers.CQRS.Accounts.Events
{
    public class AccountPasswordChanged
    {
        public DateTime LastPasswordChangeAt { set; get; }
        public string Password { set; get; }
    }
}
