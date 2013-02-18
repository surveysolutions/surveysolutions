namespace Main.Core.Events.User
{
    using System;

    using Ncqrs.Eventing.Storage;

    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:UserLocked")]
    public class UserLocked : UserBaseEvent
    {
        public Guid PublicKey { get; set; }
    }
}