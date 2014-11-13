using System;
using Ncqrs.Eventing.Storage;

// ReSharper disable once CheckNamespace
namespace Main.Core.Events.User
{
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:UserUnlocked")]
    public class UserUnlocked : UserBaseEvent
    {
        public Guid PublicKey { get; set; }
    }
}