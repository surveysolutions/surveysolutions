
using System;
namespace WB.UI.Designer.Providers.CQRS.Accounts.Events
{
    public class AccountPasswordChanged
    {
        public DateTime LastPasswordChangeAt { set; get; }
        public string Password { set; get; }
    }
}
