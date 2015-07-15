using System;
using Ncqrs.Eventing.Storage;

// ReSharper disable once CheckNamespace
namespace Main.Core.Events.User
{
    [EventName("RavenQuestionnaire.Core:Events:UserUnlocked")]
    public class UserUnlocked
    {
        public Guid PublicKey { get; set; }
    }
}