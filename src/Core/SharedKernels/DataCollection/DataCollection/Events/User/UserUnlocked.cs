using System;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;

// ReSharper disable once CheckNamespace
namespace Main.Core.Events.User
{
    [Obsolete]
    public class UserUnlocked : IEvent
    {
        public Guid PublicKey { get; set; }
    }
}