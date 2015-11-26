using System;
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Events
{
    public class AccountLocked : ILiteEvent
    {
        public DateTime LastLockedOutAt { get; set; }
    }
}
