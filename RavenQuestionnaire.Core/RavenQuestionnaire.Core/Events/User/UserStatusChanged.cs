using System;
using Ncqrs.Eventing.Storage;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Events.User
{
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:UserStatusChanged")]
    public class UserStatusChanged
    {
        public Guid PublicKey { get; set; }
        public bool IsLocked { set; get; }
    }
}
