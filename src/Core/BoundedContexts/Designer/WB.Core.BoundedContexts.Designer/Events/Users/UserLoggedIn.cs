using System;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Events
{
    public class UserLoggedIn : IEvent
    {
        public DateTime LastLoginAt { get; set; }
    }
}
