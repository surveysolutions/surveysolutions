using System;
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Events
{
    public class UserLoggedIn : ILiteEvent
    {
        public DateTime LastLoginAt { get; set; }
    }
}
