using System;
using Ncqrs.Eventing.Storage;

// ReSharper disable once CheckNamespace
namespace Main.Core.Events.User
{
    [EventName("RavenQuestionnaire.Core:Events:UserLocked")]
    public class UserLocked : UserBaseEvent
    {
        public Guid PublicKey { get; set; }
    }
}