namespace Main.Core.Events.User
{
    using System;

    using Ncqrs.Eventing.Storage;

    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:UserUnlocked")]
    public class UserUnlocked : UserBaseEvent
    {
        public Guid PublicKey { get; set; }
    }
}