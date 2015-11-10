using System;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.EventBus.Lite;

// ReSharper disable once CheckNamespace
namespace Main.Core.Events.User
{
    public class UserLocked : ILiteEvent
    {
        public Guid PublicKey { get; set; }
    }
}