using System;
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Events
{
    public class AccountLoginFailed : ILiteEvent
    {
        public DateTime FailedPasswordWindowStartedAt { get; set; }
    }
}
