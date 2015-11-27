using System;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Events
{
    public class AccountLoginFailed : IEvent
    {
        public DateTime FailedPasswordWindowStartedAt { get; set; }
    }
}
