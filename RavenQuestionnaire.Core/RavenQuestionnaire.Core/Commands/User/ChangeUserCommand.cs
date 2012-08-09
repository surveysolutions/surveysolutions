using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.User
{

    [Serializable]
    [MapsToAggregateRootMethod(typeof(UserAR), "ChangeUser")]
    public class ChangeUserCommand : CommandBase
    {
        [AggregateRootId]
        public Guid PublicKey { get; set; }

        public string UserName { get; set; }
        public string Password { get;set; }
        public string Email { get; set; }
        public UserRoles[] Roles { get;set; }
        public bool IsLocked { get; set; }
        public UserLight Supervisor { get; set; }

        public ChangeUserCommand(){}

        public ChangeUserCommand(Guid publicKey, string email, UserRoles[] roles, bool isLocked)
            : base(publicKey)
        {
            PublicKey = publicKey;
            Email = email;
            Roles = roles;
            IsLocked = isLocked;
            
        }

    }
}
