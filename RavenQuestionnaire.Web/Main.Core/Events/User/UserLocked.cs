using System;
using Ncqrs.Eventing.Storage;

namespace Main.Core.Events.User
{
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:UserLocked")]
    public class UserLocked : UserBaseEvent
    {
        public Guid PublicKey { get; set; }
    }
}