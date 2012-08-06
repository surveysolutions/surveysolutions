using System;
using Ncqrs.Eventing.Storage;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Events.User
{
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:UserCreated")]
    public class NewUserCreated
    {
        public Guid PublicKey { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsLocked { set; get; }

        public UserRoles[] Roles { set; get; }

    }
}
