using System;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Events
{
    public class AccountOnlineUpdated : IEvent
    {
        public DateTime LastActivityAt { get; set; }
    }
}
