using System;
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Events
{
    public class AccountOnlineUpdated : ILiteEvent
    {
        public DateTime LastActivityAt { get; set; }
    }
}
