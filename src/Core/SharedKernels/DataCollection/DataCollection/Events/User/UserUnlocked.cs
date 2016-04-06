using System;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;

// ReSharper disable once CheckNamespace
namespace Main.Core.Events.User
{
    [Obsolete("This event must be deleted after all clients who STARTED data collection with version 5.6 or earlier will fade away. " +
             "Pay attentions on the word 'STARTED'." +
             "If the client started with 5.5 and had been updated later to 5.8 he still might have the event which need to be handled")]
    public class UserUnlocked : IEvent
    {
        public Guid PublicKey { get; set; }
    }
}