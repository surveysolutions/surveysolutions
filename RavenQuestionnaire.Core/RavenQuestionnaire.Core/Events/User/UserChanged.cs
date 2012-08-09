using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using Ncqrs.Eventing.Storage;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Events.User
{
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:UserChanged")]
    public class UserChanged
    {
        [AggregateRootId]
        public Guid PublicKey { get; set; }

        public string Email { get; set; }
        public bool IsLocked { set; get; }
        
        //is not propogated now
        public UserRoles[] Roles { set; get; }
    }
}
