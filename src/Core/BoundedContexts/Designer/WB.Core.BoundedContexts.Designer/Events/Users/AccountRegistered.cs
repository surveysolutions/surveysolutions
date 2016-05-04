﻿using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Events
{
    public class AccountRegistered : IEvent
    {
        public string ApplicationName { set; get; }
        public string UserName { set; get; }
        public string Email { set; get; }
        public string ConfirmationToken { set; get; }

        public System.DateTime CreatedDate { get; set; }
    }
}
