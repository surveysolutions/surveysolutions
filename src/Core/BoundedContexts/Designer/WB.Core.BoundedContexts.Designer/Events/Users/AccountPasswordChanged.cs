
using System;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Events
{
    public class AccountPasswordChanged : IEvent
    {
        public DateTime LastPasswordChangeAt { set; get; }
        public string Password { set; get; }
    }
}
